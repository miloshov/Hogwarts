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
    public async Task<ActionResult<object>> Get(
        int page = 1, 
        int pageSize = 10, 
        string search = "", 
        string sortBy = "ime", 
        bool ascending = true)
    {
        try
        {
            var query = _context.Zaposleni
                .Include(z => z.Odsek)
                .Where(z => z.IsActive);

            // Search functionality
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(z => 
                    z.Ime.Contains(search) ||
                    z.Prezime.Contains(search) ||
                    z.Email.Contains(search) ||
                    (z.Pozicija != null && z.Pozicija.Contains(search)));
            }

            // Sorting
            query = sortBy.ToLower() switch
            {
                "prezime" => ascending ? query.OrderBy(z => z.Prezime) : query.OrderByDescending(z => z.Prezime),
                "email" => ascending ? query.OrderBy(z => z.Email) : query.OrderByDescending(z => z.Email),
                "pozicija" => ascending ? query.OrderBy(z => z.Pozicija) : query.OrderByDescending(z => z.Pozicija),
                "datumzaposlenja" => ascending ? query.OrderBy(z => z.DatumZaposlenja) : query.OrderByDescending(z => z.DatumZaposlenja),
                _ => ascending ? query.OrderBy(z => z.Ime) : query.OrderByDescending(z => z.Ime)
            };

            var totalCount = await query.CountAsync();

            var zaposleni = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
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
                    OdsekNaziv = z.Odsek != null ? z.Odsek.Naziv : null,
                    z.IsActive,
                    z.DatumKreiranja
                })
                .ToListAsync();

            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            return Ok(new
            {
                data = zaposleni,
                pagination = new
                {
                    currentPage = page,
                    pageSize,
                    totalCount,
                    totalPages,
                    hasNext = page < totalPages,
                    hasPrevious = page > 1
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"GreÅ¡ka pri dobijanju zaposlenih: {ex.Message}");
        }
    }

    // GET: api/zaposleni/all (without pagination for dropdowns)
    [HttpGet("all")]
    [Authorize(Roles = "SuperAdmin,HRManager,TeamLead")]
    public async Task<ActionResult<IEnumerable<object>>> GetAll()
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
            return StatusCode(500, $"GreÅ¡ka pri dobijanju zaposlenih: {ex.Message}");
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
                return NotFound("Zaposleni nije pronaÄ‘en.");

            // Proveri dozvole - zaposleni moÅ¾e da vidi samo sebe
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
            return StatusCode(500, $"GreÅ¡ka pri dobijanju zaposlenog: {ex.Message}");
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

            // Proveri da li email veÄ‡ postoji
            var postojeciZaposleni = await _context.Zaposleni
                .AnyAsync(z => z.Email == noviZaposleni.Email && z.IsActive);

            if (postojeciZaposleni)
            {
                return BadRequest("Zaposleni sa datim email-om veÄ‡ postoji.");
            }

            noviZaposleni.DatumKreiranja = DateTime.Now;
            noviZaposleni.IsActive = true;

            _context.Zaposleni.Add(noviZaposleni);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = noviZaposleni.Id }, noviZaposleni);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"GreÅ¡ka pri dodavanju zaposlenog: {ex.Message}");
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
                return NotFound("Zaposleni nije pronaÄ‘en.");

            // Proveri dozvole
            if (currentUserRole == UserRoles.Zaposleni)
            {
                var currentUser = await _context.Korisnici
                    .FirstOrDefaultAsync(k => k.Id == currentUserId);
                
                if (currentUser?.ZaposleniId != id)
                {
                    return Forbid("Nemate dozvolu za izmenu ovih podataka.");
                }

                // Zaposleni moÅ¾e da menja samo odreÄ‘ena polja
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
            return StatusCode(500, $"GreÅ¡ka pri aÅ¾uriranju zaposlenog: {ex.Message}");
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
                return NotFound("Zaposleni nije pronaÄ‘en.");

            // Soft delete - samo oznaÄiti kao neaktivnog
            zaposleni.IsActive = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"GreÅ¡ka pri brisanju zaposlenog: {ex.Message}");
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
                return NotFound("Podaci o zaposlenom nisu pronaÄ‘eni.");
            }

            return Ok(currentUser.Zaposleni);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"GreÅ¡ka pri dobijanju liÄnih podataka: {ex.Message}");
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