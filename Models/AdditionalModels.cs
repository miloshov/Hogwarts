using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hogwarts.Models
{
    // Model za plate zaposlenih
    public class Plata
    {
        public int Id { get; set; }

        [Required]
        public int ZaposleniId { get; set; }
        public virtual Zaposleni Zaposleni { get; set; } = null!;

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Osnovna plata mora biti pozitivna")]
        public decimal Osnovna { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Bonusi moraju biti pozitivni")]
        public decimal Bonusi { get; set; } = 0;

        [Range(0, double.MaxValue, ErrorMessage = "Otkazi moraju biti pozitivni")]
        public decimal Otkazi  { get; set; } = 0;

        [NotMapped]
        public decimal Neto => Osnovna + Bonusi - Otkazi;

        [Required]
        [StringLength(10)]
        public string Period { get; set; } = string.Empty; // Format: "2025-01"

        public DateTime DatumKreiranja { get; set; } = DateTime.UtcNow;
        public string? Napomene { get; set; }
    }

    // Model za zahteve za odmor
    public class ZahtevZaOdmor
    {
        public int Id { get; set; }

        [Required]
        public int ZaposleniId { get; set; }
        public virtual Zaposleni Zaposleni { get; set; } = null!;

        [Required]
        public DateTime DatumOd { get; set; }

        [Required]
        public DateTime DatumDo { get; set; }

        [StringLength(500)]
        public string? Razlog { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = StatusZahteva.NaCekanju;

        [StringLength(50)]
        public string TipOdmora { get; set; } = Hogwarts.Models.TipOdmora.Godisnji;

        public DateTime DatumZahteva { get; set; } = DateTime.UtcNow;
        public DateTime? DatumOdgovora { get; set; }
        public int? OdobrioKorisnikId { get; set; }
        public string? NapomenaOdgovora { get; set; }

        // Izračunaj broj dana - DODAO [NotMapped]!
        [NotMapped]
        public int BrojDana => (DatumDo - DatumOd).Days + 1;
    }

    // Model za odseke/departmane
    public class Odsek
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Naziv { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Opis { get; set; }

        public DateTime DatumKreiranja { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        // Relacije
        public virtual ICollection<Zaposleni> Zaposleni { get; set; } = new List<Zaposleni>();
    }

    // Enum konstante
    public static class StatusZahteva
    {
        public const string NaCekanju = "NaCekanju";
        public const string Odobren = "Odobren";
        public const string Odbacen = "Odbacen";
    }

    public static class TipOdmora
    {
        public const string Godisnji = "Godisnji";
        public const string Bolovanje = "Bolovanje";
        public const string Porodicni = "Porodicni";
        public const string Studijski = "Studijski";
        public const string Neplaceni = "Neplaceni";
    }

    // DTO klase za API
    public class ZahtevZaOdmorDto
    {
        [Required]
        public int ZaposleniId { get; set; }

        [Required]
        public DateTime DatumOd { get; set; }

        [Required]
        public DateTime DatumDo { get; set; }

        public string? Razlog { get; set; }
        public string TipOdmora { get; set; } = Hogwarts.Models.TipOdmora.Godisnji;
    }

    public class PlataDto
    {
        [Required]
        public int ZaposleniId { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Osnovna { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Bonusi { get; set; } = 0;

        [Range(0, double.MaxValue)]
        public decimal Otkazi  { get; set; } = 0;

        [Required]
        public string Period { get; set; } = string.Empty;

        public string? Napomene { get; set; }
    }

    public class OdgovorNaZahtevDto
    {
        public string? Napomena { get; set; }
    }
}