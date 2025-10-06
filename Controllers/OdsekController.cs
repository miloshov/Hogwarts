using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using Hogwarts.Data;
using Hogwarts.Models;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OdsekController : ControllerBase
{
    private readonly HogwartsContext _context;

    public OdsekController(HogwartsContext context)
    {
        _context = context;
    }

    // GET: api/odsek
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> Get()
    {
        try
        {
            var odseci = await _context.Odseci
                .Where(o => o.IsActive)
                .Select(o => new
                {
                    o.Id,
                    o.Naziv,
                    o.Opis,
                    o.DatumKreiranja,
                    BrojZaposlenih = o.Zaposleni.Count(z => z.IsActive)
                })
                .OrderBy(o => o.Naziv)
                .ToListAsync();

            return Ok(odseci);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"GreÅ¡ka pri dobijanju odseka: {ex.Message}");
        }
    }

    // GET: api/odsek/5
    [HttpGet("{id}")]
    public async Task<ActionResult<object>> Get(int id)
    {
        try
        {
            var odsek = await _context.Odseci
                .Include(o => o.Zaposleni.Where(z => z.IsActive))
                .FirstOrDefaultAsync(o => o.Id == id && o.IsActive);

            if (odsek == null)
                return NotFound("Odsek nije pronaÄ‘en.");

            var result = new
            {
                odsek.Id,
                odsek.Naziv,
                odsek.Opis,
                odsek.DatumKreiranja,
                Zaposleni = odsek.Zaposleni.Select(z => new
                {
                    z.Id,
                    z.PunoIme,
                    z.Email,
                    z.Pozicija
                })
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"GreÅ¡ka pri dobijanju odseka: {ex.Message}");
        }
    }

    // POST: api/odsek
    [HttpPost]
    [Authorize(Roles = "SuperAdmin,HRManager")]
    public async Task<ActionResult<Odsek>> Post([FromBody] OdsekDto odsekDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Proveri da li odsek sa istim nazivom veÄ‡ postoji
            var postojeciOdsek = await _context.Odseci
                .AnyAsync(o => o.Naziv.ToLower() == odsekDto.Naziv.ToLower() && o.IsActive);

            if (postojeciOdsek)
            {
                return BadRequest("Odsek sa datim nazivom veÄ‡ postoji.");
            }

            var noviOdsek = new Odsek
            {
                Naziv = odsekDto.Naziv,
                Opis = odsekDto.Opis,
                DatumKreiranja = DateTime.UtcNow,
                IsActive = true
            };

            _context.Odseci.Add(noviOdsek);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = noviOdsek.Id }, noviOdsek);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"GreÅ¡ka pri dodavanju odseka: {ex.Message}");
        }
    }

    // PUT: api/odsek/5
    [HttpPut("{id}")]
    [Authorize(Roles = "SuperAdmin,HRManager")]
    public async Task<IActionResult> Put(int id, [FromBody] OdsekDto odsekDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var postojeciOdsek = await _context.Odseci
                .FirstOrDefaultAsync(o => o.Id == id && o.IsActive);

            if (postojeciOdsek == null)
                return NotFound("Odsek nije pronaÄ‘en.");

            // Proveri da li postoji drugi odsek sa istim nazivom
            var duplikatNaziv = await _context.Odseci
                .AnyAsync(o => o.Id != id && o.Naziv.ToLower() == odsekDto.Naziv.ToLower() && o.IsActive);

            if (duplikatNaziv)
            {
                return BadRequest("Odsek sa datim nazivom veÄ‡ postoji.");
            }

            postojeciOdsek.Naziv = odsekDto.Naziv;
            postojeciOdsek.Opis = odsekDto.Opis;

            await _context.SaveChangesAsync();
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"GreÅ¡ka pri aÅ¾uriranju odseka: {ex.Message}");
        }
    }

    // DELETE: api/odsek/5
    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin,HRManager")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var odsek = await _context.Odseci
                .Include(o => o.Zaposleni)
                .FirstOrDefaultAsync(o => o.Id == id && o.IsActive);

            if (odsek == null)
                return NotFound("Odsek nije pronaÄ‘en.");

            // Proveri da li odsek ima aktivne zaposlene
            var imaAktivneZaposlene = odsek.Zaposleni.Any(z => z.IsActive);
            if (imaAktivneZaposlene)
            {
                return BadRequest("Ne moÅ¾ete obrisati odsek koji ima aktivne zaposlene. Prvo premestite zaposlene u drugi odsek.");
            }

            // Soft delete
            odsek.IsActive = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"GreÅ¡ka pri brisanju odseka: {ex.Message}");
        }
    }

    // GET: api/odsek/5/zaposleni
    [HttpGet("{id}/zaposleni")]
    public async Task<ActionResult<IEnumerable<object>>> GetZaposleni(int id)
    {
        try
        {
            var odsek = await _context.Odseci
                .FirstOrDefaultAsync(o => o.Id == id && o.IsActive);

            if (odsek == null)
                return NotFound("Odsek nije pronaÄ‘en.");

            var zaposleni = await _context.Zaposleni
                .Where(z => z.OdsekId == id && z.IsActive)
                .Select(z => new
                {
                    z.Id,
                    z.PunoIme,
                    z.Email,
                    z.Pozicija,
                    z.DatumZaposlenja
                })
                .OrderBy(z => z.PunoIme)
                .ToListAsync();

            return Ok(zaposleni);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"GreÅ¡ka pri dobijanju zaposlenih odseka: {ex.Message}");
        }
    }
}

// DTO klasa za odsek
public class OdsekDto
{
    [Required]
    [StringLength(100)]
    public string Naziv { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Opis { get; set; }
}