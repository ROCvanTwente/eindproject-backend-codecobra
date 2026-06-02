using backend.Data;
using backend.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
        sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null)));

builder.Services.Configure<IdentityOptions>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
});

builder.Services.AddAuthorization();
builder.Services.AddIdentityApiEndpoints<IdentityUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontends", builder =>
    {
        builder.WithOrigins(
            "https://eindproject-frontend-codecobra.vercel.app",      // Mobile
            "https://eindproject-frontend-codecobra-c6ez.vercel.app",   // Web
            "http://localhost:5173"                                    // Local
        )
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
    });
});

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
app.UseRouting();               // ? EERST routing
app.UseCors("AllowFrontends");  // ? DAN CORS
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
// Map Identity endpoints with CORS enabled
app.MapIdentityApi<IdentityUser>()
   .RequireCors("AllowFrontends");


app.Run();
