using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hogwarts.Models
{
    // Klasa koja predstavlja podatke o zaposlenom u HR sistemu
    public class Zaposleni
    {
        // Jedinstveni identifikator zaposlenog
        public int Id { get; set; } 

        // Osobni podaci
        [Required]
        [StringLength(50)]
        public string Ime { get; set; } = string.Empty; // Ime zaposlenog
        
        [Required]
        [StringLength(50)]
        public string Prezime { get; set; } = string.Empty; // Prezime zaposlenog
        
        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty; // Kontakt email
        
        [StringLength(100)]
        public string Pozicija { get; set; } = string.Empty; // Pozicija ili odeljenje

        // Datum kada je zaposleni započeo radni odnos
        public DateTime DatumZaposlenja { get; set; }

        // Dodatni podaci o zaposlenom
        public DateTime DatumRodjenja { get; set; } // Datum rođenja
        
        [StringLength(50)]
        public string ImeOca { get; set; } = string.Empty; // Ime oca
        
        [StringLength(13)]
        public string JMBG { get; set; } = string.Empty; // Jedinstveni Matični Broj Građana
        
        [StringLength(200)]
        public string Adresa { get; set; } = string.Empty; // Stalna adresa
        
        [StringLength(20)]
        public string BrojTelefon { get; set; } = string.Empty; // Kontakt telefon

        // Dodatna polja
        public bool IsActive { get; set; } = true; // Da li je zaposleni aktivan
        public DateTime DatumKreiranja { get; set; } = DateTime.UtcNow;
        
        // Relacije sa odsekom
        public int? OdsekId { get; set; }
        public virtual Odsek? Odsek { get; set; }

        // Relacije sa platama i zahtevima
        public virtual ICollection<Plata> Plate { get; set; } = new List<Plata>();
        public virtual ICollection<ZahtevZaOdmor> ZahteviZaOdmor { get; set; } = new List<ZahtevZaOdmor>();

        // Computed properties - DODAO [NotMapped]!
        [NotMapped]
        public string PunoIme => $"{Ime} {Prezime}";
        [NotMapped]
        public int Godine => DateTime.UtcNow.Year - DatumRodjenja.Year;
    }
}