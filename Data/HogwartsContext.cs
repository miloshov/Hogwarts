using Microsoft.EntityFrameworkCore;
using Hogwarts.Models;

namespace Hogwarts.Data
{
    public class HogwartsContext : DbContext
    {
        public HogwartsContext(DbContextOptions<HogwartsContext> options) : base(options)
        {
        }

        // DbSet za svaki entitet u aplikaciji
        public DbSet<Zaposleni> Zaposleni { get; set; }
        public DbSet<Korisnik> Korisnici { get; set; }
        public DbSet<Plata> Plate { get; set; }
        public DbSet<ZahtevZaOdmor> ZahteviZaOdmor { get; set; }
        public DbSet<Odsek> Odseci { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Konfiguracija za Zaposleni
            modelBuilder.Entity<Zaposleni>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Ime).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Prezime).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.JMBG).HasMaxLength(13);
                entity.Property(e => e.BrojTelefon).HasMaxLength(20);
                entity.Property(e => e.Adresa).HasMaxLength(200);
                entity.Property(e => e.ImeOca).HasMaxLength(50);
                entity.Property(e => e.Pozicija).HasMaxLength(100);

                // Dodaj relacije
                entity.HasOne(e => e.Odsek)
                      .WithMany(o => o.Zaposleni)
                      .HasForeignKey(e => e.OdsekId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Konfiguracija za Korisnik
            modelBuilder.Entity<Korisnik>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.Role).IsRequired().HasMaxLength(20);

                entity.HasIndex(e => e.UserName).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();

                // Relacija sa zaposlenim
                entity.HasOne(k => k.Zaposleni)
                      .WithOne()
                      .HasForeignKey<Korisnik>(k => k.ZaposleniId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Konfiguracija za Plata
            modelBuilder.Entity<Plata>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Osnovna).HasColumnType("decimal(10,2)");
                entity.Property(e => e.Bonusi).HasColumnType("decimal(10,2)");
                entity.Property(e => e.Period).IsRequired().HasMaxLength(10);

                entity.HasOne(p => p.Zaposleni)
                      .WithMany(z => z.Plate)
                      .HasForeignKey(p => p.ZaposleniId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Konfiguracija za ZahtevZaOdmor
            modelBuilder.Entity<ZahtevZaOdmor>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Razlog).HasMaxLength(500);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
                entity.Property(e => e.TipOdmora).HasMaxLength(50);

                entity.HasOne(z => z.Zaposleni)
                      .WithMany(zap => zap.ZahteviZaOdmor)
                      .HasForeignKey(z => z.ZaposleniId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Konfiguracija za Odsek
            modelBuilder.Entity<Odsek>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Naziv).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Opis).HasMaxLength(500);
            });

            // Seed podaci za poÄetno pokretanje
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Osnovni odseci
            modelBuilder.Entity<Odsek>().HasData(
                new Odsek { Id = 1, Naziv = "IT", Opis = "Informacione tehnologije" },
                new Odsek { Id = 2, Naziv = "HR", Opis = "Ljudski resursi" },
                new Odsek { Id = 3, Naziv = "Finansije", Opis = "Finansijski sektor" },
                new Odsek { Id = 4, Naziv = "Marketing", Opis = "Marketing i prodaja" }
            );

            // Admin korisnik
            modelBuilder.Entity<Korisnik>().HasData(
                new Korisnik
                {
                    Id = 1,
                    UserName = "admin",
                    Email = "admin@hogwarts.rs",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    Role = "SuperAdmin",
                    IsActive = true,
                    DatumRegistracije = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );
        }
    }
}