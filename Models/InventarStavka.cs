using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hogwarts.Models
{
    public class InventarStavka
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Naziv { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Kategorija { get; set; } = string.Empty;

        [StringLength(100)]
        public string? SeriskiBroj { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Vrednost { get; set; }

        [Required]
        public DateTime DatumNabavke { get; set; }

        [Required]
        [StringLength(200)]
        public string Lokacija { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Stanje { get; set; } = "Novo"; // Novo, Korisceno, Pokvareno, Izgubljeno

        [StringLength(1000)]
        public string? Opis { get; set; }

        [StringLength(500)]
        public string? QRKod { get; set; }

        [StringLength(200)]
        public string? Slika { get; set; }

        // Dodeljivanje zaposlenom
        public int? ZaposleniId { get; set; }
        [ForeignKey("ZaposleniId")]
        public virtual Zaposleni? Zaposleni { get; set; }

        public DateTime? DatumDodeljivanja { get; set; }
        public DateTime? DatumVracanja { get; set; }

        [StringLength(500)]
        public string? NapomenaKoriscenja { get; set; }

        // Audit fields
        [Required]
        public DateTime DatumKreiranja { get; set; } = DateTime.UtcNow;

        public DateTime? DatumIzmene { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        [StringLength(100)]
        public string? KreiraOd { get; set; }

        [StringLength(100)]
        public string? IzmenioKorisnik { get; set; }

        // Computed properties
        [NotMapped]
        public bool JeDodeljena => ZaposleniId.HasValue && !DatumVracanja.HasValue;

        [NotMapped]
        public string StatusDisplay => JeDodeljena ? 
            $"Dodeljena - {Zaposleni?.PunoIme}" : 
            "Dostupna";

        [NotMapped]
        public int DanaUKoriscenju => JeDodeljena && DatumDodeljivanja.HasValue ? 
            (DateTime.UtcNow - DatumDodeljivanja.Value).Days : 0;
    }

    // Enum za standardne kategorije
    public static class InventarKategorije
    {
        public const string Racunari = "Racunari";
        public const string Telefoni = "Telefoni";
        public const string Namestaj = "Namestaj";
        public const string Vozila = "Vozila";
        public const string Oprema = "Oprema";
        public const string Software = "Software";
        public const string Ostalo = "Ostalo";

        public static List<string> SveKategorije => new()
        {
            Racunari, Telefoni, Namestaj, Vozila, Oprema, Software, Ostalo
        };
    }

    // Enum za stanja
    public static class InventarStanja
    {
        public const string Novo = "Novo";
        public const string Korisceno = "Korisceno";
        public const string Pokvareno = "Pokvareno";
        public const string Izgubljeno = "Izgubljeno";
        public const string Otpisano = "Otpisano";

        public static List<string> SvaStanja => new()
        {
            Novo, Korisceno, Pokvareno, Izgubljeno, Otpisano
        };
    }
}