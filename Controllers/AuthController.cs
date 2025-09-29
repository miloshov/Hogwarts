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
            // PronaÄ‘i korisnika
            var korisnik = await _context.Korisnici
                .Include(k => k.Zaposleni)
                .FirstOrDefaultAsync(k => k.UserName == request.UserName && k.IsActive);

            if (korisnik == null)
            {
                return Unauthorized("Neispravno korisniÄko ime ili Å¡ifra.");
            }

            // Proveri Å¡ifru
            if (!BCrypt.Net.BCrypt.Verify(request.Password, korisnik.PasswordHash))
            {
                return Unauthorized("Neispravno korisniÄko ime ili Å¡ifra.");
            }

            // GeneriÅ¡i JWT token
            var token = GenerateJwtToken(korisnik);

            // Updateuj poslednje prijavljivanje
            korisnik.PoslednjePrijavljivanje = DateTime.Now;
            await _context.SaveChangesAsync();

            var response = new LoginResponse
            {
                Token = token,
                UserName = korisnik.UserName,
                Email = korisnik.Email,
                Role = korisnik.Role,
                ZaposleniId = korisnik.ZaposleniId,
                ExpiresAt = DateTime.Now.AddHours(8)
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"GreÅ¡ka pri prijavljivanju: {ex.Message}");
        }
    }

    // POST: api/auth/register
    [HttpPost("register")]
    public async Task<ActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            // Proveri da li korisnik veÄ‡ postoji
            var postojeciKorisnik = await _context.Korisnici
                .AnyAsync(k => k.UserName == request.UserName || k.Email == request.Email);

            if (postojeciKorisnik)
            {
                return BadRequest("Korisnik sa datim korisniÄkim imenom ili email-om veÄ‡ postoji.");
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
                DatumRegistracije = DateTime.Now
            };

            _context.Korisnici.Add(noviKorisnik);
            await _context.SaveChangesAsync();

            return Ok("Korisnik je uspeÅ¡no registrovan.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"GreÅ¡ka pri registraciji: {ex.Message}");
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
                return NotFound("Korisnik nije pronaÄ‘en.");
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
            return StatusCode(500, $"GreÅ¡ka pri dobijanju profila: {ex.Message}");
        }
    }

    private string GenerateJwtToken(Korisnik korisnik)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, korisnik.Id.ToString()),
            new Claim(ClaimTypes.Name, korisnik.UserName),
            new Claim(ClaimTypes.Email, korisnik.Email),
            new Claim(ClaimTypes.Role, korisnik.Role)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("TvojaJakaSifraZaPotpisivanjeJWT"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: null,
            audience: null,
            claims: claims,
            expires: DateTime.Now.AddHours(8),
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