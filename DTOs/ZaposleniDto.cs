using System.ComponentModel.DataAnnotations;
using Hogwarts.Models;

namespace Hogwarts.DTOs
{
    // DTO za vraćanje podataka o zaposlenom (GET requests)
    public class ZaposleniDto
    {
        public int Id { get; set; }
        public string Ime { get; set; } = string.Empty;
        public string Prezime { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Pozicija { get; set; } = string.Empty;
        public DateTime DatumZaposlenja { get; set; }
        public DateTime DatumRodjenja { get; set; }
        public string ImeOca { get; set; } = string.Empty;
        public string JMBG { get; set; } = string.Empty;
        public string Adresa { get; set; } = string.Empty;
        public string BrojTelefon { get; set; } = string.Empty;
        public string? ProfileImageUrl { get; set; }
        public Gender Pol { get; set; }
        public bool IsActive { get; set; }
        public DateTime DatumKreiranja { get; set; }
        
        // Calculated properties
        public string PunoIme { get; set; } = string.Empty;
        public int Godine { get; set; }
        public string AvatarUrl { get; set; } = string.Empty;
        
        // Odsek info (bez kružne reference)
        public int? OdsekId { get; set; }
        public string? OdsekNaziv { get; set; }
    }

    // DTO za kreiranje/ažuriranje zaposlenog (POST/PUT requests)
    public class CreateZaposleniDto
    {
        [Required]
        [StringLength(50)]
        public string Ime { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string Prezime { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string Pozicija { get; set; } = string.Empty;
        
        public DateTime DatumZaposlenja { get; set; }
        public DateTime DatumRodjenja { get; set; }
        
        [StringLength(50)]
        public string ImeOca { get; set; } = string.Empty;
        
        [StringLength(13)]
        public string JMBG { get; set; } = string.Empty;
        
        [StringLength(200)]
        public string Adresa { get; set; } = string.Empty;
        
        [StringLength(20)]
        public string BrojTelefon { get; set; } = string.Empty;
        
        [Required]
        public Gender Pol { get; set; } = Gender.Muski;
        
        public int? OdsekId { get; set; }
    }

    // Jednostavan DTO za odsek (bez zaposlenih da se izbegne kružna referenca)
    public class OdsekDto
    {
        public int Id { get; set; }
        public string Naziv { get; set; } = string.Empty;
        public string? Opis { get; set; }
        public DateTime DatumKreiranja { get; set; }
        public bool IsActive { get; set; }
        
        // Broj zaposlenih u odseku
        public int BrojZaposlenih { get; set; }
    }
}