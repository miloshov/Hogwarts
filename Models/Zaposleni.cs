using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hogwarts.Models
{
    // Enum za pol zaposlenog
    public enum Gender
    {
        Muski = 1,
        Zenski = 2
    }

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
        
        // 🔄 PROMENIO: staro string polje u PozicijaNaziv (legacy support)
        [StringLength(100)]
        public string PozicijaNaziv { get; set; } = string.Empty; // Legacy pozicija kao string

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

        // 🆕 NOVE KOLONE ZA SLIKE
        [StringLength(500)]
        public string? ProfileImageUrl { get; set; } = null; // Putanja do profilne slike

        [Required]
        public Gender Pol { get; set; } = Gender.Muski; // Pol zaposlenog za placeholder slike

        // Dodatna polja
        public bool IsActive { get; set; } = true; // Da li je zaposleni aktivan
        public DateTime DatumKreiranja { get; set; } = DateTime.UtcNow;
        
        // Relacije sa odsekom
        public int? OdsekId { get; set; }
        public virtual Odsek? Odsek { get; set; }

        // 🏗️ HIJERARHIJSKE RELACIJE
        public int? PozicijaId { get; set; }
        public virtual Pozicija? Pozicija { get; set; }

        public int? NadredjeniId { get; set; }
        public virtual Zaposleni? Nadredjeni { get; set; }

        // Relacije sa platama i zahtevima
        public virtual ICollection<Plata> Plate { get; set; } = new List<Plata>();
        public virtual ICollection<ZahtevZaOdmor> ZahteviZaOdmor { get; set; } = new List<ZahtevZaOdmor>();
        
        // 🏗️ HIJERARHIJSKE KOLEKCIJE
        public virtual ICollection<Zaposleni> Podredjeni { get; set; } = new List<Zaposleni>();

        // Computed properties - DODAO [NotMapped]!
        [NotMapped]
        public string PunoIme => $"{Ime} {Prezime}";
        
        [NotMapped]
        public int Godine => DateTime.UtcNow.Year - DatumRodjenja.Year;

        // 🔄 BACKWARD COMPATIBILITY - computed property za legacy kod
        [NotMapped]
        public string PozicijaDisplay => Pozicija?.Naziv ?? PozicijaNaziv ?? "Nedefinirano";

        // 🆕 NOVA COMPUTED PROPERTY ZA AVATAR
        [NotMapped]
        public string AvatarUrl => !string.IsNullOrEmpty(ProfileImageUrl) 
            ? ProfileImageUrl 
            : GetDefaultAvatarUrl();

        private string GetDefaultAvatarUrl()
        {
            return Pol == Gender.Muski 
                ? "/images/avatars/default-male.png" 
                : "/images/avatars/default-female.png";
        }
    }
}