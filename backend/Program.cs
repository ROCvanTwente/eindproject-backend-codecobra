using backend.Data;
using backend.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Database Connection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
        sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null)));

// Identity Options
builder.Services.Configure<IdentityOptions>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
});

// Identity + API Endpoints
builder.Services.AddAuthorization();
builder.Services.AddIdentityApiEndpoints<IdentityUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>();

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins(
                  "http://localhost:3000",
                  "http://127.0.0.1:3000",
                       "http://localhost:5173",
                       "http://127.0.0.1:5173",
                       "http://localhost:5174",
                       "http://127.0.0.1:5174",
                       "https://eindproject-frontend-codecobra.vercel.app", // Mobile
                       "https://eindproject-frontend-codecobra-c6ez.vercel.app") // Web
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
    });
});

// Controllers
builder.Services.AddControllersWithViews();

// Custom Services
builder.Services.AddScoped<IQRCodeStatisticService, QRCodeStatisticService>();

var app = builder.Build();

// Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseCors("FrontendPolicy");

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapIdentityApi<IdentityUser>();

app.Run();
