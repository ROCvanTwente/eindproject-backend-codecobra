using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using backend.Models;

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
    public DbSet<Media> Medias { get; set; }
	public DbSet<Pronunciation> Pronunciations { get; set; }
    public DbSet<UserActionLog> UserActionLogs { get; internal set; }

	protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // QRCode -> QRCodeStatistic relatie
        builder.Entity<QRCode>()
            .HasMany(q => q.Statistics)
            .WithOne(s => s.QRCode)
            .HasForeignKey(s => s.QRCodeId)
            .OnDelete(DeleteBehavior.Cascade);

        // QRCode -> Media relatie
        builder.Entity<QRCode>()
            .HasMany(q => q.Medias)
            .WithOne(m => m.QRCode)
            .HasForeignKey(m => m.QRCodeId)
            .OnDelete(DeleteBehavior.Cascade);

        // QRCode -> TourStop relatie
        builder.Entity<QRCode>()
            .HasMany<TourStop>()
            .WithOne(t => t.QRCode)
            .HasForeignKey(t => t.QRCodeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
