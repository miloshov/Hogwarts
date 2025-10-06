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
            Console.WriteLine($"Login attempt for user: {request.UserName}");
            
            // Pronađi korisnika
            var korisnik = await _context.Korisnici
                .Include(k => k.Zaposleni)
                .FirstOrDefaultAsync(k => k.UserName == request.UserName && k.IsActive);

            if (korisnik == null)
            {
                Console.WriteLine($"User not found: {request.UserName}");
                return Unauthorized("Neispravno korisničko ime ili šifra.");
            }

            // Proveri šifru
            if (!BCrypt.Net.BCrypt.Verify(request.Password, korisnik.PasswordHash))
            {
                Console.WriteLine($"Invalid password for user: {request.UserName}");
                return Unauthorized("Neispravno korisničko ime ili šifra.");
            }

            // Generiši JWT token
            var token = GenerateJwtToken(korisnik);
            Console.WriteLine($"Token generated for user: {korisnik.UserName}");

            // Updateuj poslednje prijavljivanje
            korisnik.PoslednjePrijavljivanje = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var response = new LoginResponse
            {
                Token = token,
                UserName = korisnik.UserName,
                Email = korisnik.Email,
                Role = korisnik.Role,
                ZaposleniId = korisnik.ZaposleniId,
                ExpiresAt = DateTime.UtcNow.AddHours(GetTokenExpiryHours())
            };

            Console.WriteLine($"Login successful for user: {korisnik.UserName}");
            return Ok(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Login error: {ex.Message}");
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
                DatumRegistracije = DateTime.UtcNow
            };

            _context.Korisnici.Add(noviKorisnik);
            await _context.SaveChangesAsync();

            return Ok("Korisnik je uspešno registrovan.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Registration error: {ex.Message}");
            return StatusCode(500, $"Greška pri registraciji: {ex.Message}");
        }
    }

    // GET: api/auth/profile
    [HttpGet("profile")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<ActionResult<object>> GetProfile()
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
            Console.WriteLine($"Get profile error: {ex.Message}");
            return StatusCode(500, $"Greška pri dobijanju profila: {ex.Message}");
        }
    }

    // ✅ DODANO: Token validation endpoint
    [HttpGet("validate-token")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<ActionResult> ValidateToken()
    {
        try
        {
            var userId = GetCurrentUserId();
            var korisnik = await _context.Korisnici
                .FirstOrDefaultAsync(k => k.Id == userId && k.IsActive);

            if (korisnik == null)
            {
                return Unauthorized("Token nije valjan.");
            }

            return Ok(new { IsValid = true, UserId = userId });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Token validation error: {ex.Message}");
            return Unauthorized("Token nije valjan.");
        }
    }

    // ✅ DODANO: Refresh token endpoint
    [HttpPost("refresh")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<ActionResult<LoginResponse>> RefreshToken()
    {
        try
        {
            var userId = GetCurrentUserId();
            var korisnik = await _context.Korisnici
                .Include(k => k.Zaposleni)
                .FirstOrDefaultAsync(k => k.Id == userId && k.IsActive);

            if (korisnik == null)
            {
                return Unauthorized("Korisnik nije pronađen.");
            }

            // Generiši novi token
            var newToken = GenerateJwtToken(korisnik);

            var response = new LoginResponse
            {
                Token = newToken,
                UserName = korisnik.UserName,
                Email = korisnik.Email,
                Role = korisnik.Role,
                ZaposleniId = korisnik.ZaposleniId,
                ExpiresAt = DateTime.UtcNow.AddHours(GetTokenExpiryHours())
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Refresh token error: {ex.Message}");
            return StatusCode(500, $"Greška pri osvežavanju tokena: {ex.Message}");
        }
    }

    // ✅ ISPRAVKA: Popravljena JWT token generacija
    private string GenerateJwtToken(Korisnik korisnik)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, korisnik.Id.ToString()),
            new Claim(ClaimTypes.Name, korisnik.UserName),
            new Claim(ClaimTypes.Email, korisnik.Email),
            new Claim(ClaimTypes.Role, korisnik.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(GetJwtKey()));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // ✅ ISPRAVKA: Dodaj issuer i audience
        var token = new JwtSecurityToken(
            issuer: GetJwtIssuer(),        // ✅ Dodano
            audience: GetJwtAudience(),    // ✅ Dodano
            claims: claims,
            expires: DateTime.UtcNow.AddHours(GetTokenExpiryHours()),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // ✅ Helper methods za konfiguraciju
    private string GetJwtKey()
    {
        return _configuration["Jwt:Key"] ?? "HogwartsSecretKey123456789AbcDefGhiJklMnoPqrStUvWxYz!@#$%^&*()";
    }

    private string GetJwtIssuer()
    {
        return _configuration["Jwt:Issuer"] ?? "Hogwarts";
    }

    private string GetJwtAudience()
    {
        return _configuration["Jwt:Audience"] ?? "Hogwarts";
    }

    private int GetTokenExpiryHours()
    {
        return _configuration.GetValue<int>("Jwt:ExpiryInHours", 24);
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
