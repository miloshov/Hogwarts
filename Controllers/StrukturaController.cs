using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Hogwarts.Data;
using Hogwarts.Models;

namespace Hogwarts.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StrukturaController : ControllerBase
    {
        private readonly HogwartsContext _context;

        public StrukturaController(HogwartsContext context)
        {
            _context = context;
        }

        // GET: api/struktura/org-chart
        [HttpGet("org-chart")]
        public async Task<ActionResult<List<OrgChartNodeDto>>> GetOrgChart()
        {
            try
            {
                // Učitaj sve zaposlene sa relacijama
                var zaposleni = await _context.Zaposleni
                    .Include(z => z.Pozicija)
                    .Include(z => z.Odsek)
                    .Include(z => z.Podredjeni)
                    .Where(z => z.IsActive)
                    .ToListAsync();

                // Konvertuj u DTO-ove
                var orgNodes = zaposleni.Select(z => new OrgChartNodeDto
                {
                    Id = z.Id,
                    Ime = z.Ime,
                    Prezime = z.Prezime,
                    PunoIme = z.PunoIme,
                    Email = z.Email,
                    Pozicija = z.PozicijaDisplay,
                    Odsek = z.Odsek?.Naziv,
                    AvatarUrl = z.AvatarUrl,
                    NadredjeniId = z.NadredjeniId,
                    PozicijaNivo = z.Pozicija?.Nivo ?? 99,
                    PozicijaBoja = z.Pozicija?.Boja ?? "#95a5a6",
                    DatumZaposlenja = z.DatumZaposlenja,
                    IsActive = z.IsActive,
                    Podredjeni = new List<OrgChartNodeDto>()
                }).ToList();

                // Izgradi hijerarhiju - počni od top-level (bez nadređenog)
                var topLevel = orgNodes.Where(n => n.NadredjeniId == null).ToList();
                
                foreach (var node in topLevel)
                {
                    BuildHierarchy(node, orgNodes);
                }

                // Sortiraj po poziciji (nizži nivo = viša pozicija)
                var sortedTopLevel = topLevel.OrderBy(n => n.PozicijaNivo).ThenBy(n => n.Ime).ToList();

                return Ok(sortedTopLevel);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri učitavanju org chart-a: {ex.Message}");
            }
        }

        // GET: api/struktura/pozicije
        [HttpGet("pozicije")]
        public async Task<ActionResult<List<Pozicija>>> GetPozicije()
        {
            try
            {
                var pozicije = await _context.Pozicije
                    .Where(p => p.IsActive)
                    .OrderBy(p => p.Nivo)
                    .ThenBy(p => p.Naziv)
                    .ToListAsync();

                return Ok(pozicije);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri učitavanju pozicija: {ex.Message}");
            }
        }

        // PUT: api/struktura/hijerarhija
        [HttpPut("hijerarhija")]
        public async Task<IActionResult> UpdateHijerarhiju([FromBody] UpdateHijerarhijeDto dto)
        {
            try
            {
                var zaposleni = await _context.Zaposleni.FindAsync(dto.ZaposleniId);
                if (zaposleni == null)
                {
                    return NotFound($"Zaposleni sa ID {dto.ZaposleniId} nije pronađen");
                }

                // Proveri da zaposleni ne postavlja sebe kao nadređenog (direktno ili indirektno)
                if (dto.NoviNadredjeniId.HasValue)
                {
                    if (await IsCircularReference(dto.ZaposleniId, dto.NoviNadredjeniId.Value))
                    {
                        return BadRequest("Nije moguće postaviti hijerarhiju koja pravi ciklus");
                    }
                }

                // Ažuriraj nadređenog
                zaposleni.NadredjeniId = dto.NoviNadredjeniId;

                // Ažuriraj poziciju ako je poslana
                if (dto.NovaPozicijaId.HasValue)
                {
                    var pozicija = await _context.Pozicije.FindAsync(dto.NovaPozicijaId.Value);
                    if (pozicija == null)
                    {
                        return BadRequest($"Pozicija sa ID {dto.NovaPozicijaId.Value} ne postoji");
                    }
                    zaposleni.PozicijaId = dto.NovaPozicijaId.Value;
                }

                await _context.SaveChangesAsync();

                return Ok(new { message = "Hijerarhija je uspešno ažurirana" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri ažuriranju hijerarhije: {ex.Message}");
            }
        }

        // POST: api/struktura/pozicije
        [HttpPost("pozicije")]
        public async Task<ActionResult<Pozicija>> CreatePozicija([FromBody] Pozicija pozicija)
        {
            try
            {
                // Proveri da li pozicija sa istim nazivom već postoji
                var postojeca = await _context.Pozicije
                    .AnyAsync(p => p.Naziv.ToLower() == pozicija.Naziv.ToLower() && p.IsActive);
                
                if (postojeca)
                {
                    return BadRequest($"Pozicija sa nazivom '{pozicija.Naziv}' već postoji");
                }

                pozicija.DatumKreiranja = DateTime.UtcNow;
                pozicija.IsActive = true;

                _context.Pozicije.Add(pozicija);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetPozicije), new { id = pozicija.Id }, pozicija);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri kreiranju pozicije: {ex.Message}");
            }
        }

        // GET: api/struktura/zaposleni/{id}/tim
        [HttpGet("zaposleni/{id}/tim")]
        public async Task<ActionResult<List<OrgChartNodeDto>>> GetZaposleniTim(int id)
        {
            try
            {
                var zaposleni = await _context.Zaposleni
                    .Include(z => z.Podredjeni)
                        .ThenInclude(p => p.Pozicija)
                    .Include(z => z.Podredjeni)
                        .ThenInclude(p => p.Odsek)
                    .FirstOrDefaultAsync(z => z.Id == id && z.IsActive);

                if (zaposleni == null)
                {
                    return NotFound($"Zaposleni sa ID {id} nije pronađen");
                }

                var tim = zaposleni.Podredjeni
                    .Where(p => p.IsActive)
                    .Select(p => new OrgChartNodeDto
                    {
                        Id = p.Id,
                        Ime = p.Ime,
                        Prezime = p.Prezime,
                        PunoIme = p.PunoIme,
                        Email = p.Email,
                        Pozicija = p.PozicijaDisplay,
                        Odsek = p.Odsek?.Naziv,
                        AvatarUrl = p.AvatarUrl,
                        NadredjeniId = p.NadredjeniId,
                        PozicijaNivo = p.Pozicija?.Nivo ?? 99,
                        PozicijaBoja = p.Pozicija?.Boja ?? "#95a5a6",
                        DatumZaposlenja = p.DatumZaposlenja,
                        IsActive = p.IsActive
                    })
                    .OrderBy(p => p.PozicijaNivo)
                    .ThenBy(p => p.Ime)
                    .ToList();

                return Ok(tim);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri učitavanju tima: {ex.Message}");
            }
        }

        #region Helper Methods

        private void BuildHierarchy(OrgChartNodeDto parent, List<OrgChartNodeDto> allNodes)
        {
            var children = allNodes.Where(n => n.NadredjeniId == parent.Id).ToList();
            
            foreach (var child in children)
            {
                parent.Podredjeni.Add(child);
                BuildHierarchy(child, allNodes); // Rekurzivno izgradi hijerarhiju
            }

            // Sortiraj podređene po poziciji
            parent.Podredjeni = parent.Podredjeni
                .OrderBy(p => p.PozicijaNivo)
                .ThenBy(p => p.Ime)
                .ToList();
        }

        private async Task<bool> IsCircularReference(int zaposleniId, int nadredjeniId)
        {
            // Proveri da li postavljanje nadredjeniId kao nadređenog za zaposleniId pravi ciklus
            var current = nadredjeniId;
            var visited = new HashSet<int>();

            while (current > 0)
            {
                if (current == zaposleniId)
                {
                    return true; // Ciklus pronađen
                }

                if (visited.Contains(current))
                {
                    break; // Beskonačna petlja, prekini
                }

                visited.Add(current);

                var nadredjeni = await _context.Zaposleni
                    .Where(z => z.Id == current)
                    .Select(z => z.NadredjeniId)
                    .FirstOrDefaultAsync();

                current = nadredjeni ?? 0;
            }

            return false;
        }

        #endregion
    }
}