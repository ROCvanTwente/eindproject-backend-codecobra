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

    // DbSets voor QR Code
    public DbSet<QRCode> QRCodes { get; set; }
    public DbSet<QRCodeStatistic> QRCodeStatistics { get; set; }
    public DbSet<TourStop> TourStops { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // QRCode -> QRCodeStatistic relatie
        builder.Entity<QRCode>()
            .HasMany(q => q.Statistics)
            .WithOne(s => s.QRCode)
            .HasForeignKey(s => s.QRCodeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
