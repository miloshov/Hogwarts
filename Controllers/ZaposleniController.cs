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
                    telefon = z.BrojTelefon, // Frontend oÄekuje "telefon"
                    z.Adresa,
                    datumRodjenja = z.DatumRodjenja.ToString("yyyy-MM-dd"), // Frontend oÄekuje string format
                    datumZaposlenja = z.DatumZaposlenja.ToString("yyyy-MM-dd"), // Frontend oÄekuje string format
                    z.Pozicija,
                    odeljenje = z.Odeljenje, // Frontend oÄekuje "odeljenje" kao string
                    trenutnaPlata = z.TrenutnaPlata // NOVO polje
                })
                .ToListAsync();

            return Ok(zaposleni);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"GreÅ¡ka pri dohvatanju zaposlenih: {ex.Message}");
        }
    }

    // GET: api/zaposleni/5
    [HttpGet("{id}")]
    public async Task<ActionResult<object>> Get(int id)
    {
        try
        {
            var zaposleni = await _context.Zaposleni
                .Include(z => z.Odsek)
                .Where(z => z.Id == id && z.IsActive)
                .Select(z => new
                {
                    z.Id,
                    z.Ime,
                    z.Prezime,
                    z.Email,
                    telefon = z.BrojTelefon,
                    z.Adresa,
                    datumRodjenja = z.DatumRodjenja.ToString("yyyy-MM-dd"),
                    datumZaposlenja = z.DatumZaposlenja.ToString("yyyy-MM-dd"),
                    z.Pozicija,
                    odeljenje = z.Odeljenje,
                    trenutnaPlata = z.TrenutnaPlata
                })
                .FirstOrDefaultAsync();

            if (zaposleni == null)
                return NotFound("Zaposleni nije pronaÄ‘en.");

            return Ok(zaposleni);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"GreÅ¡ka pri dohvatanju zaposlenog: {ex.Message}");
        }
    }

    // POST: api/zaposleni
    [HttpPost]
    public async Task<ActionResult<Zaposleni>> Post([FromBody] ZaposleniCreateDto noviZaposleni)
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

            var zaposleni = new Zaposleni
            {
                Ime = noviZaposleni.Ime,
                Prezime = noviZaposleni.Prezime,
                Email = noviZaposleni.Email,
                BrojTelefon = noviZaposleni.Telefon,
                Adresa = noviZaposleni.Adresa,
                DatumRodjenja = DateTime.Parse(noviZaposleni.DatumRodjenja),
                DatumZaposlenja = DateTime.Parse(noviZaposleni.DatumZaposlenja),
                Pozicija = noviZaposleni.Pozicija,
                Odeljenje = noviZaposleni.Odeljenje,
                TrenutnaPlata = noviZaposleni.TrenutnaPlata,
                DatumKreiranja = DateTime.Now,
                IsActive = true
            };

            _context.Zaposleni.Add(zaposleni);
            await _context.SaveChangesAsync();

            // Vrati isti format kao GET
            var result = new
            {
                zaposleni.Id,
                zaposleni.Ime,
                zaposleni.Prezime,
                zaposleni.Email,
                telefon = zaposleni.BrojTelefon,
                zaposleni.Adresa,
                datumRodjenja = zaposleni.DatumRodjenja.ToString("yyyy-MM-dd"),
                datumZaposlenja = zaposleni.DatumZaposlenja.ToString("yyyy-MM-dd"),
                zaposleni.Pozicija,
                odeljenje = zaposleni.Odeljenje,
                trenutnaPlata = zaposleni.TrenutnaPlata
            };

            return CreatedAtAction(nameof(Get), new { id = zaposleni.Id }, result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"GreÅ¡ka pri dodavanju zaposlenog: {ex.Message}");
        }
    }

    // PUT: api/zaposleni/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, [FromBody] ZaposleniUpdateDto azuriraniZaposleni)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var postojeciZaposleni = await _context.Zaposleni
                .FirstOrDefaultAsync(z => z.Id == id && z.IsActive);

            if (postojeciZaposleni == null)
                return NotFound("Zaposleni nije pronaÄ‘en.");

            // AÅ¾uriraj samo prosleÄ‘ena polja
            if (!string.IsNullOrEmpty(azuriraniZaposleni.Ime))
                postojeciZaposleni.Ime = azuriraniZaposleni.Ime;
            
            if (!string.IsNullOrEmpty(azuriraniZaposleni.Prezime))
                postojeciZaposleni.Prezime = azuriraniZaposleni.Prezime;
            
            if (!string.IsNullOrEmpty(azuriraniZaposleni.Email))
                postojeciZaposleni.Email = azuriraniZaposleni.Email;
            
            if (!string.IsNullOrEmpty(azuriraniZaposleni.Telefon))
                postojeciZaposleni.BrojTelefon = azuriraniZaposleni.Telefon;
            
            if (!string.IsNullOrEmpty(azuriraniZaposleni.Adresa))
                postojeciZaposleni.Adresa = azuriraniZaposleni.Adresa;
            
            if (!string.IsNullOrEmpty(azuriraniZaposleni.DatumRodjenja))
                postojeciZaposleni.DatumRodjenja = DateTime.Parse(azuriraniZaposleni.DatumRodjenja);
            
            if (!string.IsNullOrEmpty(azuriraniZaposleni.DatumZaposlenja))
                postojeciZaposleni.DatumZaposlenja = DateTime.Parse(azuriraniZaposleni.DatumZaposlenja);
            
            if (!string.IsNullOrEmpty(azuriraniZaposleni.Pozicija))
                postojeciZaposleni.Pozicija = azuriraniZaposleni.Pozicija;
            
            if (!string.IsNullOrEmpty(azuriraniZaposleni.Odeljenje))
                postojeciZaposleni.Odeljenje = azuriraniZaposleni.Odeljenje;
            
            if (azuriraniZaposleni.TrenutnaPlata.HasValue)
                postojeciZaposleni.TrenutnaPlata = azuriraniZaposleni.TrenutnaPlata.Value;

            await _context.SaveChangesAsync();

            // Vrati aÅ¾urirani objekat u istom formatu kao GET
            var result = new
            {
                postojeciZaposleni.Id,
                postojeciZaposleni.Ime,
                postojeciZaposleni.Prezime,
                postojeciZaposleni.Email,
                telefon = postojeciZaposleni.BrojTelefon,
                postojeciZaposleni.Adresa,
                datumRodjenja = postojeciZaposleni.DatumRodjenja.ToString("yyyy-MM-dd"),
                datumZaposlenja = postojeciZaposleni.DatumZaposlenja.ToString("yyyy-MM-dd"),
                postojeciZaposleni.Pozicija,
                odeljenje = postojeciZaposleni.Odeljenje,
                trenutnaPlata = postojeciZaposleni.TrenutnaPlata
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"GreÅ¡ka pri aÅ¾uriranju zaposlenog: {ex.Message}");
        }
    }

    // DELETE: api/zaposleni/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var zaposleni = await _context.Zaposleni.FindAsync(id);
            if (zaposleni == null)
                return NotFound("Zaposleni nije pronaÄ‘en.");

            // Soft delete - oznaÄavamo kao neaktivan
            zaposleni.IsActive = false;
            await _context.SaveChangesAsync();

            return Ok("Zaposleni je uspeÅ¡no uklonjen.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"GreÅ¡ka pri brisanju zaposlenog: {ex.Message}");
        }
    }
}

// DTO klase za API
public class ZaposleniCreateDto
{
    public string Ime { get; set; } = string.Empty;
    public string Prezime { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefon { get; set; } = string.Empty;
    public string Adresa { get; set; } = string.Empty;
    public string DatumRodjenja { get; set; } = string.Empty;
    public string DatumZaposlenja { get; set; } = string.Empty;
    public string Pozicija { get; set; } = string.Empty;
    public string Odeljenje { get; set; } = string.Empty;
    public decimal TrenutnaPlata { get; set; }
}

public class ZaposleniUpdateDto
{
    public string? Ime { get; set; }
    public string? Prezime { get; set; }
    public string? Email { get; set; }
    public string? Telefon { get; set; }
    public string? Adresa { get; set; }
    public string? DatumRodjenja { get; set; }
    public string? DatumZaposlenja { get; set; }
    public string? Pozicija { get; set; }
    public string? Odeljenje { get; set; }
    public decimal? TrenutnaPlata { get; set; }
}