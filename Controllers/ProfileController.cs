using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using Hogwarts.Data;
using Hogwarts.Models;
using BC = BCrypt.Net.BCrypt;

namespace Hogwarts.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly HogwartsContext _context;

        public ProfileController(HogwartsContext context)
        {
            _context = context;
        }

        // GET: api/profile
        [HttpGet]
        public async Task<ActionResult<object>> GetUserProfile()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized("User ID not found in token");
                }

                // Dobij podatke o zaposlenom
                var zaposleni = await _context.Zaposleni
                    .Include(z => z.Pozicija)
                    .Include(z => z.Odsek)
                    .Include(z => z.Nadredjeni)
                        .ThenInclude(n => n!.Pozicija)
                    .FirstOrDefaultAsync(z => z.Id == userId && z.IsActive);

                if (zaposleni == null)
                {
                    return NotFound("Zaposleni nije pronađen");
                }

                // Dobij sekundarnog nadređenog (nadređeni od nadređenog)
                var sekundarniNadredjeni = zaposleni.Nadredjeni?.NadredjeniId != null
                    ? await _context.Zaposleni
                        .Include(z => z.Pozicija)
                        .FirstOrDefaultAsync(z => z.Id == zaposleni.Nadredjeni.NadredjeniId && z.IsActive)
                    : null;

                // Dobij trenutnu platu
                var trenutnaPlata = await _context.Plate
                    .Where(p => p.ZaposleniId == userId)
                    .OrderByDescending(p => p.Id)
                    .FirstOrDefaultAsync();

                // Dobij preostale dane odmora
                var trenutnaGodina = DateTime.Now.Year;
                var iskorisceniDani = await _context.ZahteviZaOdmor
                    .Where(z => z.ZaposleniId == userId && 
                               z.Status == "Odobreno" &&
                               z.DatumOd.Year == trenutnaGodina)
                    .SumAsync(z => (z.DatumDo - z.DatumOd).Days + 1);

                var ukupnoDana = 25; // Default broj dana godišnjeg
                var preostaliDani = ukupnoDana - iskorisceniDani;

                // Dobij zaduženi inventar - sa osnovnim svojstvima
                var inventar = new List<object>();
                try
                {
                    // Proveravamo koja svojstva postoje u InventarStavka
                    inventar = await _context.InventarStavke
                        .Where(i => i.ZaposleniId == userId)
                        .Select(i => new
                        {
                            i.Id,
                            i.Naziv,
                            // Uklanjamo SerijskiBroj jer ne postoji
                            i.Kategorija,
                            i.DatumDodeljivanja
                        })
                        .ToListAsync<object>();
                }
                catch
                {
                    // Ako InventarStavke ne postoji, ostavi prazan niz
                }

                var profile = new
                {
                    // Lični podaci
                    Id = zaposleni.Id,
                    Ime = zaposleni.Ime,
                    Prezime = zaposleni.Prezime,
                    PunoIme = zaposleni.PunoIme,
                    Email = zaposleni.Email,
                    AvatarUrl = zaposleni.AvatarUrl ?? "",

                    // Poslovni podaci
                    Odsek = zaposleni.Odsek?.Naziv ?? "",
                    Pozicija = zaposleni.Pozicija?.Naziv ?? "",
                    PozicijaDisplay = zaposleni.PozicijaDisplay ?? "",
                    Plata = trenutnaPlata?.Osnovna ?? 0,
                    PreostaliDaniOdmora = Math.Max(preostaliDani, 0),
                    PrimarniMenadzer = zaposleni.Nadredjeni != null ? new
                    {
                        Id = zaposleni.Nadredjeni.Id,
                        PunoIme = zaposleni.Nadredjeni.PunoIme,
                        Pozicija = zaposleni.Nadredjeni.Pozicija?.Naziv ?? ""
                    } : null,
                    SekundarniMenadzer = sekundarniNadredjeni != null ? new
                    {
                        Id = sekundarniNadredjeni.Id,
                        PunoIme = sekundarniNadredjeni.PunoIme,
                        Pozicija = sekundarniNadredjeni.Pozicija?.Naziv ?? ""
                    } : null,

                    // Opšti podaci
                    DatumZaposlenja = zaposleni.DatumZaposlenja,
                    StazUKompaniji = CalculateYearsOfService(zaposleni.DatumZaposlenja),
                    ZaduzeniInventar = inventar
                };

                return Ok(profile);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri dobijanju korisničkog profila: {ex.Message}");
            }
        }

        // PUT: api/profile
        [HttpPut]
        public async Task<ActionResult> UpdateUserProfile([FromBody] UpdateProfileDto dto)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized("User ID not found in token");
                }

                var zaposleni = await _context.Zaposleni.FindAsync(userId);
                if (zaposleni == null)
                {
                    return NotFound("Zaposleni nije pronađen");
                }

                // Ažuriraj samo email
                if (!string.IsNullOrWhiteSpace(dto.Email))
                {
                    zaposleni.Email = dto.Email;
                }

                await _context.SaveChangesAsync();
                return Ok(new { message = "Profil je uspešno ažuriran" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri ažuriranju profila: {ex.Message}");
            }
        }

        // PUT: api/profile/password
        [HttpPut("password")]
        public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized("User ID not found in token");
                }

                var korisnik = await _context.Korisnici.FindAsync(userId);
                if (korisnik == null)
                {
                    return NotFound("Korisnik nije pronađen");
                }

                // Proveri staru lozinku
                if (!BC.Verify(dto.StaraLozinka, korisnik.PasswordHash))
                {
                    return BadRequest("Neispravna stara lozinka");
                }

                // Ažuriraj lozinku
                korisnik.PasswordHash = BC.HashPassword(dto.NovaLozinka);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Lozinka je uspešno promenjena" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri promeni lozinke: {ex.Message}");
            }
        }

        private string CalculateYearsOfService(DateTime datumZaposlenja)
        {
            var timeSpan = DateTime.Now - datumZaposlenja;
            var years = timeSpan.Days / 365;
            var months = (timeSpan.Days % 365) / 30;
            
            if (years > 0)
            {
                return $"{years} godina{(years == 1 ? "" : (years < 5 ? "e" : "a"))}{(months > 0 ? $" i {months} mesec{(months == 1 ? "" : (months < 5 ? "a" : "i"))}" : "")}";
            }
            else if (months > 0)
            {
                return $"{months} mesec{(months == 1 ? "" : (months < 5 ? "a" : "i"))}";
            }
            else
            {
                return "Manje od mesec dana";
            }
        }
    }

    // DTOs
    public class UpdateProfileDto
    {
        public string? Email { get; set; }
    }

    public class ChangePasswordDto
    {
        [Required]
        public string StaraLozinka { get; set; } = string.Empty;
        
        [Required]
        public string NovaLozinka { get; set; } = string.Empty;
    }
}