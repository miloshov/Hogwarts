using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Hogwarts.Data;
using Hogwarts.Models;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PlataController : ControllerBase
{
    private readonly HogwartsContext _context;

    public PlataController(HogwartsContext context)
    {
        _context = context;
    }

    // GET: api/plata
    [HttpGet]
    [Authorize(Roles = "SuperAdmin,HRManager")]
    public async Task<ActionResult<IEnumerable<object>>> Get([FromQuery] string? period = null)
    {
        try
        {
            var query = _context.Plate
                .Include(p => p.Zaposleni)
                .ThenInclude(z => z.Odsek)
                .AsQueryable();

            if (!string.IsNullOrEmpty(period))
            {
                query = query.Where(p => p.Period == period);
            }

            var plate = await query
                .Select(p => new
                {
                    p.Id,
                    p.ZaposleniId,
                    ZaposleniIme = p.Zaposleni.PunoIme,
                    p.Osnovna,
                    p.Bonusi,
                    p.Otkazi,
                    p.Neto,
                    p.Period,
                    p.DatumKreiranja,
                    p.Napomene,
                    OdsekNaziv = p.Zaposleni.Odsek != null ? p.Zaposleni.Odsek.Naziv : null
                })
                .OrderByDescending(p => p.DatumKreiranja)
                .ToListAsync();

            return Ok(plate);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"GreÅ¡ka pri dobijanju plata: {ex.Message}");
        }
    }

    // GET: api/plata/zaposleni/5
    [HttpGet("zaposleni/{zaposleniId}")]
    public async Task<ActionResult<IEnumerable<object>>> GetByZaposleni(int zaposleniId)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var currentUserRole = GetCurrentUserRole();

            // Proveri dozvole
            if (currentUserRole == UserRoles.Zaposleni)
            {
                var currentUser = await _context.Korisnici
                    .FirstOrDefaultAsync(k => k.Id == currentUserId);
                
                if (currentUser?.ZaposleniId != zaposleniId)
                {
                    return Forbid("Nemate dozvolu za pristup ovim podacima.");
                }
            }

            var plate = await _context.Plate
                .Include(p => p.Zaposleni)
                .Where(p => p.ZaposleniId == zaposleniId)
                .Select(p => new
                {
                    p.Id,
                    p.Osnovna,
                    p.Bonusi,
                    p.Otkazi,
                    p.Neto,
                    p.Period,
                    p.DatumKreiranja,
                    p.Napomene
                })
                .OrderByDescending(p => p.Period)
                .ToListAsync();

            return Ok(plate);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"GreÅ¡ka pri dobijanju plata zaposlenog: {ex.Message}");
        }
    }

    // POST: api/plata
    [HttpPost]
    [Authorize(Roles = "SuperAdmin,HRManager")]
    public async Task<ActionResult<Plata>> Post([FromBody] PlataDto plataDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Proveri da li zaposleni postoji
            var zaposleni = await _context.Zaposleni
                .FirstOrDefaultAsync(z => z.Id == plataDto.ZaposleniId && z.IsActive);

            if (zaposleni == null)
            {
                return BadRequest("Zaposleni nije pronaÄ‘en.");
            }

            // Proveri da li veÄ‡ postoji plata za taj period
            var postojecaPlata = await _context.Plate
                .AnyAsync(p => p.ZaposleniId == plataDto.ZaposleniId && p.Period == plataDto.Period);

            if (postojecaPlata)
            {
                return BadRequest($"Plata za period {plataDto.Period} veÄ‡ postoji za ovog zaposlenog.");
            }

            var novaPlata = new Plata
            {
                ZaposleniId = plataDto.ZaposleniId,
                Osnovna = plataDto.Osnovna,
                Bonusi = plataDto.Bonusi,
                Otkazi = plataDto.Otkazi,
                Period = plataDto.Period,
                Napomene = plataDto.Napomene,
                DatumKreiranja = DateTime.UtcNow
            };

            _context.Plate.Add(novaPlata);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = novaPlata.Id }, novaPlata);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"GreÅ¡ka pri dodavanju plate: {ex.Message}");
        }
    }

    // PUT: api/plata/5
    [HttpPut("{id}")]
    [Authorize(Roles = "SuperAdmin,HRManager")]
    public async Task<IActionResult> Put(int id, [FromBody] PlataDto plataDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var postojecaPlata = await _context.Plate
                .FirstOrDefaultAsync(p => p.Id == id);

            if (postojecaPlata == null)
                return NotFound("Plata nije pronaÄ‘ena.");

            postojecaPlata.Osnovna = plataDto.Osnovna;
            postojecaPlata.Bonusi = plataDto.Bonusi;
            postojecaPlata.Otkazi = plataDto.Otkazi;
            postojecaPlata.Napomene = plataDto.Napomene;

            await _context.SaveChangesAsync();
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"GreÅ¡ka pri aÅ¾uriranju plate: {ex.Message}");
        }
    }

    // DELETE: api/plata/5
    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin,HRManager")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var plata = await _context.Plate
                .FirstOrDefaultAsync(p => p.Id == id);

            if (plata == null)
                return NotFound("Plata nije pronaÄ‘ena.");

            _context.Plate.Remove(plata);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"GreÅ¡ka pri brisanju plate: {ex.Message}");
        }
    }

    // GET: api/plata/statistike
    [HttpGet("statistike")]
    [Authorize(Roles = "SuperAdmin,HRManager,TeamLead")]
    public async Task<ActionResult> GetStatistike([FromQuery] string? period = null)
    {
        try
        {
            var query = _context.Plate.AsQueryable();
            
            if (!string.IsNullOrEmpty(period))
            {
                query = query.Where(p => p.Period == period);
            }

            var statistike = await query
                .GroupBy(p => 1)
                .Select(g => new
                {
                    UkupnoPlata = g.Count(),
                    ProsecnaOsnovna = g.Average(p => p.Osnovna),
                    UkupnaOsnovna = g.Sum(p => p.Osnovna),
                    UkupniBonusi = g.Sum(p => p.Bonusi),
                    UkupniOtkazi = g.Sum(p => p.Otkazi),
                    UkupnoNeto = g.Sum(p => p.Neto)
                })
                .FirstOrDefaultAsync();

            return Ok(statistike ?? new { 
                UkupnoPlata = 0, 
                ProsecnaOsnovna = 0m, 
                UkupnaOsnovna = 0m, 
                UkupniBonusi = 0m, 
                UkupniOtkazi = 0m, 
                UkupnoNeto = 0m 
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"GreÅ¡ka pri dobijanju statistika: {ex.Message}");
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