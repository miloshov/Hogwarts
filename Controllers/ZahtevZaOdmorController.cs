using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Hogwarts.Data;
using Hogwarts.Models;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ZahtevZaOdmorController : ControllerBase
{
    private readonly HogwartsContext _context;

    public ZahtevZaOdmorController(HogwartsContext context)
    {
        _context = context;
    }

    // GET: api/zahtevzaodmor
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> Get([FromQuery] string? status = null)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var currentUserRole = GetCurrentUserRole();

            var query = _context.ZahteviZaOdmor
                .Include(z => z.Zaposleni)
                .ThenInclude(zap => zap.Odsek)
                .AsQueryable();

            // Filtriranje po dozvoli
            if (currentUserRole == UserRoles.Zaposleni)
            {
                var currentUser = await _context.Korisnici
                    .FirstOrDefaultAsync(k => k.Id == currentUserId);
                
                if (currentUser?.ZaposleniId.HasValue == true)
                {
                    query = query.Where(z => z.ZaposleniId == currentUser.ZaposleniId.Value);
                }
                else
                {
                    return Ok(new List<object>()); // Vrati praznu listu ako nema povezanog zaposlenog
                }
            }
            // TeamLead može da vidi zahteve u svom odeljenju
            else if (currentUserRole == UserRoles.TeamLead)
            {
                var currentUser = await _context.Korisnici
                    .Include(k => k.Zaposleni)
                    .FirstOrDefaultAsync(k => k.Id == currentUserId);
                
                if (currentUser?.Zaposleni?.OdsekId.HasValue == true)
                {
                    query = query.Where(z => z.Zaposleni.OdsekId == currentUser.Zaposleni.OdsekId.Value);
                }
            }
            // HRManager i SuperAdmin vide sve

            // Filtriranje po statusu
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(z => z.Status == status);
            }

            var zahtevi = await query
                .Select(z => new
                {
                    z.Id,
                    z.ZaposleniId,
                    ZaposleniIme = z.Zaposleni.PunoIme,
                    OdsekNaziv = z.Zaposleni.Odsek != null ? z.Zaposleni.Odsek.Naziv : null,
                    z.DatumOd,
                    z.DatumDo,
                    z.BrojDana,
                    z.Razlog,
                    z.Status,
                    z.TipOdmora,
                    z.DatumZahteva,
                    z.DatumOdgovora,
                    z.OdobrioKorisnikId,
                    z.NapomenaOdgovora
                })
                .OrderByDescending(z => z.DatumZahteva)
                .ToListAsync();

            return Ok(zahtevi);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Greška pri dobijanju zahteva: {ex.Message}");
        }
    }

    // GET: api/zahtevzaodmor/5
    [HttpGet("{id}")]
    public async Task<ActionResult<ZahtevZaOdmor>> Get(int id)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var currentUserRole = GetCurrentUserRole();

            var zahtev = await _context.ZahteviZaOdmor
                .Include(z => z.Zaposleni)
                .ThenInclude(zap => zap.Odsek)
                .FirstOrDefaultAsync(z => z.Id == id);

            if (zahtev == null)
                return NotFound("Zahtev nije pronađen.");

            // Proveri dozvole
            if (currentUserRole == UserRoles.Zaposleni)
            {
                var currentUser = await _context.Korisnici
                    .FirstOrDefaultAsync(k => k.Id == currentUserId);
                
                if (currentUser?.ZaposleniId != zahtev.ZaposleniId)
                {
                    return Forbid("Nemate dozvolu za pristup ovom zahtevu.");
                }
            }

            return Ok(zahtev);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Greška pri dobijanju zahteva: {ex.Message}");
        }
    }

    // POST: api/zahtevzaodmor
    [HttpPost]
    public async Task<ActionResult<ZahtevZaOdmor>> Post([FromBody] ZahtevZaOdmorDto zahtevDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = GetCurrentUserId();
            var currentUserRole = GetCurrentUserRole();

            // Zaposleni može da podnosi zahtev samo za sebe
            if (currentUserRole == UserRoles.Zaposleni)
            {
                var currentUser = await _context.Korisnici
                    .FirstOrDefaultAsync(k => k.Id == currentUserId);
                
                if (currentUser?.ZaposleniId != zahtevDto.ZaposleniId)
                {
                    return Forbid("Možete podneti zahtev samo za sebe.");
                }
            }

            // Proveri da li zaposleni postoji
            var zaposleni = await _context.Zaposleni
                .FirstOrDefaultAsync(z => z.Id == zahtevDto.ZaposleniId && z.IsActive);

            if (zaposleni == null)
            {
                return BadRequest("Zaposleni nije pronađen.");
            }

            // Validacija datuma
            if (zahtevDto.DatumOd >= zahtevDto.DatumDo)
            {
                return BadRequest("Datum do mora biti posle datuma od.");
            }

            if (zahtevDto.DatumOd < DateTime.Today)
            {
                return BadRequest("Ne možete podneti zahtev za prošle datume.");
            }

            // Proveri da li postoji preklapanje sa drugim zahtevima
            var preklapanje = await _context.ZahteviZaOdmor
                .AnyAsync(z => z.ZaposleniId == zahtevDto.ZaposleniId &&
                              z.Status == StatusZahteva.Odobren &&
                              ((zahtevDto.DatumOd >= z.DatumOd && zahtevDto.DatumOd <= z.DatumDo) ||
                               (zahtevDto.DatumDo >= z.DatumOd && zahtevDto.DatumDo <= z.DatumDo)));

            if (preklapanje)
            {
                return BadRequest("Postoji preklapanje sa već odobrenim zahtevom za odmor.");
            }

            var noviZahtev = new ZahtevZaOdmor
            {
                ZaposleniId = zahtevDto.ZaposleniId,
                DatumOd = zahtevDto.DatumOd,
                DatumDo = zahtevDto.DatumDo,
                Razlog = zahtevDto.Razlog,
                TipOdmora = zahtevDto.TipOdmora,
                Status = StatusZahteva.NaCekanju,
                DatumZahteva = DateTime.Now
            };

            _context.ZahteviZaOdmor.Add(noviZahtev);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = noviZahtev.Id }, noviZahtev);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Greška pri dodavanju zahteva: {ex.Message}");
        }
    }

    // PUT: api/zahtevzaodmor/5/odobri
    [HttpPut("{id}/odobri")]
    [Authorize(Roles = "SuperAdmin,HRManager,TeamLead")]
    public async Task<IActionResult> Odobri(int id, [FromBody] OdgovorNaZahtevDto odgovor)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var zahtev = await _context.ZahteviZaOdmor
                .Include(z => z.Zaposleni)
                .FirstOrDefaultAsync(z => z.Id == id);

            if (zahtev == null)
                return NotFound("Zahtev nije pronađen.");

            if (zahtev.Status != StatusZahteva.NaCekanju)
            {
                return BadRequest("Zahtev je već obrađen.");
            }

            zahtev.Status = StatusZahteva.Odobren;
            zahtev.DatumOdgovora = DateTime.Now;
            zahtev.OdobrioKorisnikId = currentUserId;
            zahtev.NapomenaOdgovora = odgovor.Napomena;

            await _context.SaveChangesAsync();

            return Ok("Zahtev je uspešno odobren.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Greška pri odobrjavanju zahteva: {ex.Message}");
        }
    }

    // PUT: api/zahtevzaodmor/5/odbaci
    [HttpPut("{id}/odbaci")]
    [Authorize(Roles = "SuperAdmin,HRManager,TeamLead")]
    public async Task<IActionResult> Odbaci(int id, [FromBody] OdgovorNaZahtevDto odgovor)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var zahtev = await _context.ZahteviZaOdmor
                .FirstOrDefaultAsync(z => z.Id == id);

            if (zahtev == null)
                return NotFound("Zahtev nije pronađen.");

            if (zahtev.Status != StatusZahteva.NaCekanju)
            {
                return BadRequest("Zahtev je već obrađen.");
            }

            if (string.IsNullOrEmpty(odgovor.Napomena))
            {
                return BadRequest("Razlog odbacivanja je obavezan.");
            }

            zahtev.Status = StatusZahteva.Odbacen;
            zahtev.DatumOdgovora = DateTime.Now;
            zahtev.OdobrioKorisnikId = currentUserId;
            zahtev.NapomenaOdgovora = odgovor.Napomena;

            await _context.SaveChangesAsync();

            return Ok("Zahtev je odbačen.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Greška pri odbacivanju zahteva: {ex.Message}");
        }
    }

    // DELETE: api/zahtevzaodmor/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var currentUserRole = GetCurrentUserRole();

            var zahtev = await _context.ZahteviZaOdmor
                .FirstOrDefaultAsync(z => z.Id == id);

            if (zahtev == null)
                return NotFound("Zahtev nije pronađen.");

            // Samo vlasnik zahteva može da ga obriše i samo ako nije obrađen
            if (currentUserRole == UserRoles.Zaposleni)
            {
                var currentUser = await _context.Korisnici
                    .FirstOrDefaultAsync(k => k.Id == currentUserId);
                
                if (currentUser?.ZaposleniId != zahtev.ZaposleniId)
                {
                    return Forbid("Možete obrisati samo vlastite zahteve.");
                }

                if (zahtev.Status != StatusZahteva.NaCekanju)
                {
                    return BadRequest("Ne možete obrisati obrađen zahtev.");
                }
            }

            _context.ZahteviZaOdmor.Remove(zahtev);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Greška pri brisanju zahteva: {ex.Message}");
        }
    }

    // GET: api/zahtevzaodmor/statistike
    [HttpGet("statistike")]
    [Authorize(Roles = "SuperAdmin,HRManager,TeamLead")]
    public async Task<ActionResult> GetStatistike([FromQuery] int? godina = null)
    {
        try
        {
            var targetGodina = godina ?? DateTime.Now.Year;

            var statistike = await _context.ZahteviZaOdmor
                .Where(z => z.DatumOd.Year == targetGodina)
                .GroupBy(z => z.Status)
                .Select(g => new
                {
                    Status = g.Key,
                    Broj = g.Count(),
                    UkupnoDana = g.Sum(z => z.BrojDana)
                })
                .ToListAsync();

            var mesecneStatistike = await _context.ZahteviZaOdmor
                .Where(z => z.DatumOd.Year == targetGodina && z.Status == StatusZahteva.Odobren)
                .GroupBy(z => z.DatumOd.Month)
                .Select(g => new
                {
                    Mesec = g.Key,
                    BrojZahteva = g.Count(),
                    BrojDana = g.Sum(z => z.BrojDana)
                })
                .OrderBy(m => m.Mesec)
                .ToListAsync();

            return Ok(new
            {
                Godina = targetGodina,
                StatusStatistike = statistike,
                MesecneStatistike = mesecneStatistike
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Greška pri dobijanju statistika: {ex.Message}");
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

// DTO klasa za odgovor na zahtev
public class OdgovorNaZahtevDto
{
    public string? Napomena { get; set; }
}