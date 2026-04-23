using Microsoft.EntityFrameworkCore;
using DigitaleRondleiding.Models;

namespace DigitaleRondleiding.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Locatie> Locaties { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Locatie>().HasData(
                new Locatie { Id = 1, Naam = "Ingang", Beschrijving = "De hoofdingang van de rondleiding.", Volgorde = 1 },
                new Locatie { Id = 2, Naam = "Hal A", Beschrijving = "Eerste hal met exposities.", Volgorde = 2 },
                new Locatie { Id = 3, Naam = "Hal B", Beschrijving = "Tweede hal met interactieve stands.", Volgorde = 3 }
            );
        }
    }
}
