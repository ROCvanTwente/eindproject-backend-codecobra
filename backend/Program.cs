using backend.Data;
using backend.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configure Database Connection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
        sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null)));

// Configure Identity Options
builder.Services.Configure<IdentityOptions>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
});

// Configure Identity and API Endpoints
builder.Services.AddAuthorization();
builder.Services.AddIdentityApiEndpoints<IdentityUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>();

// Configure CORS for React frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins(
                  "http://localhost:3000", "http://127.0.0.1:3000", 
                  "http://localhost:5173", "http://127.0.0.1:5173",
                  "http://localhost:5174", "http://127.0.0.1:5174") 
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Needed if you use cookies for auth
    });
});

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add QRCode Statistics Service
builder.Services.AddScoped<IQRCodeStatisticService, QRCodeStatisticService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// Enable CORS
app.UseCors("FrontendPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

// Map Identity API Endpoints
app.MapIdentityApi<IdentityUser>();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
