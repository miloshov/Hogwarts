using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Hogwarts.Data;
using Hogwarts.Models;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Sve akcije zahtevaju autentifikaciju
public class ZaposleniController : ControllerBase
{
    private readonly HogwartsContext _context;

    public ZaposleniController(HogwartsContext context)
    {
        _context = context;
    }

    // GET: api/zaposleni
    [HttpGet]
    [Authorize(Roles = "SuperAdmin,HRManager,TeamLead")]
    public async Task<ActionResult<IEnumerable<object>>> Get()
    {
        try
        {
            var zaposleni = await _context.Zaposleni
                .Include(z => z.Odsek)
                .Where(z => z.IsActive)
                .Select(z => new
                {
                    z.Id,
                    z.Ime,
                    z.Prezime,
                    z.Email,
                    z.Pozicija,
                    z.DatumZaposlenja,
                    z.DatumRodjenja,
                    z.ImeOca,
                    z.JMBG,
                    z.Adresa,
                    z.BrojTelefon,
                    z.PunoIme,
                    z.Godine,
                    z.OdsekId,
                    OdsekNaziv = z.Odsek != null ? z.Odsek.Naziv : null
                })
                .ToListAsync();

            return Ok(zaposleni);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Greška pri dobijanju zaposlenih: {ex.Message}");
        }
    }

    // GET: api/zaposleni/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Zaposleni>> Get(int id)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var currentUserRole = GetCurrentUserRole();

            var zaposleni = await _context.Zaposleni
                .Include(z => z.Odsek)
                .FirstOrDefaultAsync(z => z.Id == id);

            if (zaposleni == null)
                return NotFound("Zaposleni nije pronađen.");

            // Proveri dozvole - zaposleni može da vidi samo sebe
            if (currentUserRole == UserRoles.Zaposleni)
            {
                var currentUser = await _context.Korisnici
                    .FirstOrDefaultAsync(k => k.Id == currentUserId);
                
                if (currentUser?.ZaposleniId != id)
                {
                    return Forbid("Nemate dozvolu za pristup ovim podacima.");
                }
            }

            return Ok(zaposleni);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Greška pri dobijanju zaposlenog: {ex.Message}");
        }
    }

    // POST: api/zaposleni
    [HttpPost]
    [Authorize(Roles = "SuperAdmin,HRManager")]
    public async Task<ActionResult<Zaposleni>> Post([FromBody] Zaposleni noviZaposleni)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Proveri da li email već postoji
            var postojeciZaposleni = await _context.Zaposleni
                .AnyAsync(z => z.Email == noviZaposleni.Email && z.IsActive);

            if (postojeciZaposleni)
            {
                return BadRequest("Zaposleni sa datim email-om već postoji.");
            }

            noviZaposleni.DatumKreiranja = DateTime.Now;
            noviZaposleni.IsActive = true;

            _context.Zaposleni.Add(noviZaposleni);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = noviZaposleni.Id }, noviZaposleni);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Greška pri dodavanju zaposlenog: {ex.Message}");
        }
    }

    [HttpGet("test")]
public IActionResult Test()
{
    return Ok("ZaposleniController radi!");
}

   
public async Task<IActionResult> FixAdminPassword()
    {
        try
        {
            var admin = await _context.Korisnici
                .FirstOrDefaultAsync(k => k.UserName == "admin");

            if (admin == null)
            {
                return NotFound("Admin not found");
            }

            // Hash-uj password properly
            var oldHash = admin.PasswordHash;
            admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123");

            await _context.SaveChangesAsync();

            return Ok($"Password updated! Old: {oldHash}, New: {admin.PasswordHash}");
        }
        catch (Exception ex)
        {
            return BadRequest($"Error: {ex.Message}");
        }
    }

    // PUT: api/zaposleni/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, [FromBody] Zaposleni azuriraniZaposleni)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = GetCurrentUserId();
            var currentUserRole = GetCurrentUserRole();

            var postojeciZaposleni = await _context.Zaposleni
                .FirstOrDefaultAsync(z => z.Id == id);

            if (postojeciZaposleni == null)
                return NotFound("Zaposleni nije pronađen.");

            // Proveri dozvole
            if (currentUserRole == UserRoles.Zaposleni)
            {
                var currentUser = await _context.Korisnici
                    .FirstOrDefaultAsync(k => k.Id == currentUserId);
                
                if (currentUser?.ZaposleniId != id)
                {
                    return Forbid("Nemate dozvolu za izmenu ovih podataka.");
                }

                // Zaposleni može da menja samo određena polja
                postojeciZaposleni.Email = azuriraniZaposleni.Email;
                postojeciZaposleni.Adresa = azuriraniZaposleni.Adresa;
                postojeciZaposleni.BrojTelefon = azuriraniZaposleni.BrojTelefon;
            }
            else if (currentUserRole == UserRoles.HRManager || currentUserRole == UserRoles.SuperAdmin)
            {
                // HR Manager i SuperAdmin mogu da menjaju sve
                postojeciZaposleni.Ime = azuriraniZaposleni.Ime;
                postojeciZaposleni.Prezime = azuriraniZaposleni.Prezime;
                postojeciZaposleni.Email = azuriraniZaposleni.Email;
                postojeciZaposleni.Pozicija = azuriraniZaposleni.Pozicija;
                postojeciZaposleni.DatumZaposlenja = azuriraniZaposleni.DatumZaposlenja;
                postojeciZaposleni.DatumRodjenja = azuriraniZaposleni.DatumRodjenja;
                postojeciZaposleni.ImeOca = azuriraniZaposleni.ImeOca;
                postojeciZaposleni.JMBG = azuriraniZaposleni.JMBG;
                postojeciZaposleni.Adresa = azuriraniZaposleni.Adresa;
                postojeciZaposleni.BrojTelefon = azuriraniZaposleni.BrojTelefon;
                postojeciZaposleni.OdsekId = azuriraniZaposleni.OdsekId;
            }
            else
            {
                return Forbid("Nemate dozvolu za izmenu podataka.");
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Greška pri ažuriranju zaposlenog: {ex.Message}");
        }
    }

    // DELETE: api/zaposleni/5
    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin,HRManager")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var zaposleni = await _context.Zaposleni
                .FirstOrDefaultAsync(z => z.Id == id);

            if (zaposleni == null)
                return NotFound("Zaposleni nije pronađen.");

            // Soft delete - samo označiti kao neaktivnog
            zaposleni.IsActive = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Greška pri brisanju zaposlenog: {ex.Message}");
        }
    }

    // GET: api/zaposleni/moji-podaci
    [HttpGet("moji-podaci")]
    public async Task<ActionResult> GetMojiPodaci()
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var currentUser = await _context.Korisnici
                .Include(k => k.Zaposleni)
                .ThenInclude(z => z!.Odsek)
                .FirstOrDefaultAsync(k => k.Id == currentUserId);

            if (currentUser?.Zaposleni == null)
            {
                return NotFound("Podaci o zaposlenom nisu pronađeni.");
            }

            return Ok(currentUser.Zaposleni);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Greška pri dobijanju ličnih podataka: {ex.Message}");
        }
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
    }

    private string GetCurrentUserRole()
    {
        var roleClaim = User.FindFirst(ClaimTypes.Role);
        return roleClaim?.Value ?? string.Empty;
    }
}