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
        public DbSet<Pozicija> Pozicije { get; set; }  // NOVO: DbSet za Pozicija

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
                
                // NOVO: Konf. za PozicijaNaziv (staro Pozicija polje)
                entity.Property(e => e.PozicijaNaziv).HasMaxLength(100);

                // Relacija sa Odsek
                entity.HasOne(e => e.Odsek)
                      .WithMany(o => o.Zaposleni)
                      .HasForeignKey(e => e.OdsekId)
                      .OnDelete(DeleteBehavior.SetNull);

                // NOVO: Self-referencing relacija za hijerarhiju (nadredjeni-podredjeni)
                entity.HasOne(e => e.Nadredjeni)
                      .WithMany(e => e.Podredjeni)
                      .HasForeignKey(e => e.NadredjeniId)
                      .OnDelete(DeleteBehavior.Restrict); // Restrict umesto Cascade da sprečimo circular delete

                // NOVO: Self-referencing relacija za hijerarhiju (nadredjeni-podredjeni)
                entity.HasOne(e => e.Nadredjeni)
                      .WithMany(e => e.Podredjeni)
                      .HasForeignKey(e => e.NadredjeniId)
                      .OnDelete(DeleteBehavior.Restrict); // Restrict umesto Cascade da sprečimo circular delete
            });

            // NOVO: Konfiguracija za Pozicija
            modelBuilder.Entity<Pozicija>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Naziv).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Nivo).HasMaxLength(50);
                entity.Property(e => e.Opis).HasMaxLength(500);
                
                // Jedinstveni indeks za naziv pozicije
                entity.HasIndex(e => e.Naziv).IsUnique();
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

            // Seed podaci za početno pokretanje
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

            // NOVO: Osnovne pozicije
            modelBuilder.Entity<Pozicija>().HasData(
                new Pozicija { Id = 1, Naziv = "CEO", Nivo = 1, Opis = "Chief Executive Officer", Boja = "#e74c3c" },
                new Pozicija { Id = 2, Naziv = "CTO", Nivo = 1, Opis = "Chief Technology Officer", Boja = "#e67e22" },
                new Pozicija { Id = 3, Naziv = "CFO", Nivo = 1, Opis = "Chief Financial Officer", Boja = "#f39c12" },
                new Pozicija { Id = 4, Naziv = "HR Manager", Nivo = 2, Opis = "Human Resources Manager", Boja = "#27ae60" },
                new Pozicija { Id = 5, Naziv = "IT Manager", Nivo = 2, Opis = "Information Technology Manager", Boja = "#2980b9" },
                new Pozicija { Id = 6, Naziv = "Senior Developer", Nivo = 3, Opis = "Senior Software Developer", Boja = "#8e44ad" },
                new Pozicija { Id = 7, Naziv = "Junior Developer", Nivo = 4, Opis = "Junior Software Developer", Boja = "#34495e" },
                new Pozicija { Id = 8, Naziv = "Business Analyst", Nivo = 3, Opis = "Business Analyst", Boja = "#16a085" },
                new Pozicija { Id = 9, Naziv = "QA Engineer", Nivo = 3, Opis = "Quality Assurance Engineer", Boja = "#d35400" },
                new Pozicija { Id = 10, Naziv = "Marketing Specialist", Nivo = 3, Opis = "Marketing Specialist", Boja = "#c0392b" }
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