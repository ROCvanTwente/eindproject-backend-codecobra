using backend.Data;
using backend.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
        sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null)));

builder.Services.Configure<IdentityOptions>(options =>
{
	options.SignIn.RequireConfirmedAccount = false;

	// Password policy
	options.Password.RequireDigit = true;
	options.Password.RequiredLength = 8;
	options.Password.RequireNonAlphanumeric = true;
	options.Password.RequireUppercase = true;
	options.Password.RequireLowercase = true;
	options.Password.RequiredUniqueChars = 1;

	// Lockout policy
	options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
	options.Lockout.MaxFailedAccessAttempts = 5;
	options.Lockout.AllowedForNewUsers = true;

	// User policy
	options.User.RequireUniqueEmail = true;
});

builder.Services.AddAuthorization();
builder.Services.AddIdentityApiEndpoints<IdentityUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddCors(options =>
{
	var allowedOrigins = builder.Environment.IsDevelopment()
		? new[] { "http://localhost:5173", "http://localhost:5174", "http://localhost:5000", "http://10.0.2.2:5018" }
		: new[] { "https://eindproject-frontend-codecobra.vercel.app", "https://eindproject-frontend-codecobra-c6ez.vercel.app" };

	options.AddPolicy("AllowFrontends", builder =>
	{
		builder
			.WithOrigins(allowedOrigins)
			.AllowAnyMethod()
			.AllowAnyHeader()
			.AllowCredentials();
	});
});
// Seed admin account na migraties
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    if (!await roleManager.RoleExistsAsync("Admin"))
        await roleManager.CreateAsync(new IdentityRole("Admin"));

    if (await userManager.FindByNameAsync("admin") == null)
    {
        var adminUser = new IdentityUser
        {
            UserName = "admin",
            Email = "admin@admin.com",
            EmailConfirmed = true
        };
        var result = await userManager.CreateAsync(adminUser, "Test-123");
        if (result.Succeeded)
            await userManager.AddToRoleAsync(adminUser, "Admin");
    }
}

builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IQRCodeStatisticService, QRCodeStatisticService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Maak uploads folder toegankelijk
var uploadsPath = Path.Combine(app.Environment.ContentRootPath, "uploads");
Directory.CreateDirectory(uploadsPath);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});

app.UseRouting();               // ✅ EERST routing
app.UseCors("AllowFrontends");  // ✅ DAN CORS
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
// Map Identity endpoints with CORS enabled
app.MapIdentityApi<IdentityUser>()
   .RequireCors("AllowFrontends");


app.Run();
