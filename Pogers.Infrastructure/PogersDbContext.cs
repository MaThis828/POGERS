using Microsoft.EntityFrameworkCore;
using Pogers.Core.Entities;

namespace Pogers.Infrastructure
{
    /// <summary>
    /// Datenbankzugriff auf alle Entities
    /// </summary>
    public class PogersDbContext : DbContext
    {
        public PogersDbContext(DbContextOptions<PogersDbContext> optionen) : base(optionen) { }

        public DbSet<Person> Personen => Set<Person>();
        public DbSet<Fraktion> Fraktionen => Set<Fraktion>();
        public DbSet<PersonWahlperiode> PersonWahlperioden => Set<PersonWahlperiode>();
        public DbSet<Vorgang> Vorgaenge => Set<Vorgang>();
        public DbSet<Abstimmung> Abstimmungen => Set<Abstimmung>();
        public DbSet<Stimme> Stimmen => Set<Stimme>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Id = DIP-Id, SQLite soll keine vergeben
            modelBuilder.Entity<Person>(e =>
            {
                e.Property(p => p.Id).ValueGeneratedNever();
                e.Property(p => p.Vorname).IsRequired();
                e.Property(p => p.Nachname).IsRequired();
            });

            modelBuilder.Entity<Vorgang>(e =>
            {
                e.Property(v => v.Id).ValueGeneratedNever();
                e.Property(v => v.Titel).IsRequired();
            });

            modelBuilder.Entity<Fraktion>(e =>
            {
                e.Property(f => f.Name).IsRequired();
                e.HasIndex(f => f.Name).IsUnique();
            });

            modelBuilder.Entity<PersonWahlperiode>(e =>
            {
                e.HasKey(pw => new { pw.PersonId, pw.Wahlperiode });

                e.HasOne(pw => pw.Person)
                 .WithMany(p => p.Wahlperioden)
                 .HasForeignKey(pw => pw.PersonId);

                e.HasOne(pw => pw.Fraktion)
                 .WithMany(f => f.Mitglieder)
                 .HasForeignKey(pw => pw.FraktionId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Abstimmung>(e =>
            {
                e.Property(a => a.Titel).IsRequired();

                // Fachlicher Schluessel gegen doppelte Importe
                e.HasIndex(a => new { a.Wahlperiode, a.Sitzungsnr, a.Abstimmnr }).IsUnique();

                e.HasOne(a => a.Vorgang)
                 .WithMany(v => v.Abstimmungen)
                 .HasForeignKey(a => a.VorgangId)
                 .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Stimme>(e =>
            {
                // Eine Person stimmt pro Abstimmung genau einmal
                e.HasIndex(s => new { s.AbstimmungId, s.PersonId }).IsUnique();

                e.Property(s => s.Ergebnis).HasConversion<string>();

                e.HasOne(s => s.Abstimmung)
                 .WithMany(a => a.Stimmen)
                 .HasForeignKey(s => s.AbstimmungId);

                e.HasOne(s => s.Person)
                 .WithMany(p => p.Stimmen)
                 .HasForeignKey(s => s.PersonId);
            });
        }
    }
}
