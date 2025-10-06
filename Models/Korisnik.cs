using System.ComponentModel.DataAnnotations;

namespace Hogwarts.Models
{
    // Klasa za korisnike sistema sa razliÄitim nivoima pristupa
    public class Korisnik
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Role { get; set; } = string.Empty; // SuperAdmin, HRManager, TeamLead, Zaposleni

        public bool IsActive { get; set; } = true;
        public DateTime DatumRegistracije { get; set; } = DateTime.UtcNow;
        public DateTime? PoslednjePrijavljivanje { get; set; }

        // Povezanost sa zaposlenim
        public int? ZaposleniId { get; set; }
        public virtual Zaposleni? Zaposleni { get; set; }
    }

    // Enum za role u sistemu
    public static class UserRoles
    {
        public const string SuperAdmin = "SuperAdmin";
        public const string HRManager = "HRManager";
        public const string TeamLead = "TeamLead";
        public const string Zaposleni = "Zaposleni";
    }

    // DTO klase za login
    public class LoginRequest
    {
        [Required]
        public string UserName { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int? ZaposleniId { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}