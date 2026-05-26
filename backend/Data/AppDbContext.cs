using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace backend.Data;

public class AppDbContext : IdentityDbContext<IdentityUser, IdentityRole, string>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    // DbSets voor QR Code statistieken
    public DbSet<QRCode> QRCodes { get; set; }
    public DbSet<QRCodeStatistic> QRCodeStatistics { get; set; }
}
