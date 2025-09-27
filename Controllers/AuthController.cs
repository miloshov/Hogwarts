using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.ComponentModel.DataAnnotations;
using Hogwarts.Data;
using Hogwarts.Models;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly HogwartsContext _context;
    private readonly IConfiguration _configuration;

    public AuthController(HogwartsContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    // POST: api/auth/login
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            // Pronađi korisnika
            var korisnik = await _context.Korisnici
                .Include(k => k.Zaposleni)
                .FirstOrDefaultAsync(k => k.UserName == request.UserName && k.IsActive);

            if (korisnik == null)
            {
                return Unauthorized("Neispravno korisničko ime ili šifra.");
            }

            // Proveri šifru
            if (!BCrypt.Net.BCrypt.Verify(request.Password, korisnik.PasswordHash))
            {
                return Unauthorized("Neispravno korisničko ime ili šifra.");
            }

            // Generiši JWT token
            var token = GenerateJwtToken(korisnik);

            // Updateuj poslednje prijavljivanje - ⭐ UTC
            korisnik.PoslednjePrijavljivanje = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var response = new LoginResponse
            {
                Token = token,
                UserName = korisnik.UserName,
                Email = korisnik.Email,
                Role = korisnik.Role,
                ZaposleniId = korisnik.ZaposleniId,
                ExpiresAt = DateTime.UtcNow.AddHours(24) // ⭐ UTC
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Greška pri prijavljivanju: {ex.Message}");
        }
    }

    // POST: api/auth/register
    [HttpPost("register")]
    public async Task<ActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            // Proveri da li korisnik već postoji
            var postojeciKorisnik = await _context.Korisnici
                .AnyAsync(k => k.UserName == request.UserName || k.Email == request.Email);

            if (postojeciKorisnik)
            {
                return BadRequest("Korisnik sa datim korisničkim imenom ili email-om već postoji.");
            }

            // Kreiraj novog korisnika
            var noviKorisnik = new Korisnik
            {
                UserName = request.UserName,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = request.Role ?? UserRoles.Zaposleni,
                ZaposleniId = request.ZaposleniId,
                IsActive = true,
                DatumRegistracije = DateTime.UtcNow // ⭐ UTC
            };

            _context.Korisnici.Add(noviKorisnik);
            await _context.SaveChangesAsync();

            return Ok("Korisnik je uspešno registrovan.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Greška pri registraciji: {ex.Message}");
        }
    }

    // GET: api/auth/profile
    [HttpGet("profile")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<ActionResult> GetProfile()
    {
        try
        {
            var userId = GetCurrentUserId();
            var korisnik = await _context.Korisnici
                .Include(k => k.Zaposleni)
                .ThenInclude(z => z!.Odsek)
                .FirstOrDefaultAsync(k => k.Id == userId);

            if (korisnik == null)
            {
                return NotFound("Korisnik nije pronađen.");
            }

            return Ok(new
            {
                korisnik.Id,
                korisnik.UserName,
                korisnik.Email,
                korisnik.Role,
                korisnik.ZaposleniId,
                Zaposleni = korisnik.Zaposleni != null ? new
                {
                    korisnik.Zaposleni.Id,
                    korisnik.Zaposleni.PunoIme,
                    korisnik.Zaposleni.Pozicija,
                    korisnik.Zaposleni.Email,
                    Odsek = korisnik.Zaposleni.Odsek?.Naziv
                } : null
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Greška pri dobijanju profila: {ex.Message}");
        }
    }

    // ⭐ AŽURIRANA METODA SA UTC DATETIME
    private string GenerateJwtToken(Korisnik korisnik)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, korisnik.Id.ToString()),
            new Claim(ClaimTypes.Name, korisnik.UserName),
            new Claim(ClaimTypes.Email, korisnik.Email),
            new Claim(ClaimTypes.Role, korisnik.Role)
        };

        // Čitaj ključ iz appsettings.json sa null check
        var jwtKey = _configuration["Jwt:Key"] ?? "HogwartsSecretKey123456789AbcDefGhiJklMnoPqrStUvWxYz!@#$%^&*()";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "Hogwarts",
            audience: _configuration["Jwt:Audience"] ?? "Hogwarts", 
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24), // ⭐ UTC
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
    }
}

// DTO klase za registraciju
public class RegisterRequest
{
    [Required]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    public string? Role { get; set; }
    public int? ZaposleniId { get; set; }
}