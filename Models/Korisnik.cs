using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Hogwarts.Models;

/// <summary>
/// Klasa za korisnike sistema sa različitim nivoima pristupa
/// </summary>
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
    public string Role { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime DatumRegistracije { get; set; } = DateTime.UtcNow;

    public DateTime? PoslednjePrijavljivanje { get; set; }

    public int? ZaposleniId { get; set; }

    /// <summary>
    /// Povezanost sa zaposlenim
    /// </summary>
    public virtual Zaposleni? Zaposleni { get; set; }
}

public static class UserRoles
{
    public const string SuperAdmin = "SuperAdmin";
    public const string HRManager = "HRManager";
    public const string TeamLead = "TeamLead";
    public const string Zaposleni = "Zaposleni";
}

// ✅ ISPRAVKA: LoginRequest sa JSON atributima
public class LoginRequest
{
    [Required]
    [JsonPropertyName("UserName")]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [JsonPropertyName("Password")]
    public string Password { get; set; } = string.Empty;
}

// ✅ ISPRAVKA: LoginResponse sa JSON atributima
public class LoginResponse
{
    [JsonPropertyName("Token")]
    public string Token { get; set; } = string.Empty;

    [JsonPropertyName("UserName")]
    public string UserName { get; set; } = string.Empty;

    [JsonPropertyName("Email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("Role")]
    public string Role { get; set; } = string.Empty;

    [JsonPropertyName("ZaposleniId")]
    public int? ZaposleniId { get; set; }

    [JsonPropertyName("ExpiresAt")]
    public DateTime ExpiresAt { get; set; }
}
