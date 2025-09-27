using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Hogwarts.Data;
using Hogwarts.Models;

namespace Hogwarts.Controllers
{
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

        // GET: api/Plata
        [HttpGet]
        [Authorize(Roles = "SuperAdmin,HRManager,Administrator")]
        public async Task<ActionResult<IEnumerable<Plata>>> GetPlate()
        {
            return await _context.Plate.ToListAsync();
        }

        // GET: api/Plata/5
        [HttpGet("{id}")]
        [Authorize(Roles = "SuperAdmin,HRManager,Administrator")]
        public async Task<ActionResult<Plata>> GetPlata(int id)
        {
            var plata = await _context.Plate.FindAsync(id);
            if (plata == null)
            {
                return NotFound();
            }
            return plata;
        }

        // POST: api/Plata
        [HttpPost]
        [Authorize(Roles = "SuperAdmin,HRManager")]
        public async Task<ActionResult<Plata>> PostPlata(PlataDto request)
        {
            var plata = new Plata
            {
                ZaposleniId = request.ZaposleniId,
                Osnovna = request.Osnovna,
                Bonusi = request.Bonusi,
                Otkazi = request.Otkazi,
                Period = request.Period,
                Napomene = request.Napomene
            };

            _context.Plate.Add(plata);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPlata), new { id = plata.Id }, plata);
        }

        // PUT: api/Plata/5
        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin,HRManager")]
        public async Task<IActionResult> PutPlata(int id, PlataDto request)
        {
            var plata = await _context.Plate.FindAsync(id);
            if (plata == null)
            {
                return NotFound();
            }

            plata.ZaposleniId = request.ZaposleniId;
            plata.Osnovna = request.Osnovna;
            plata.Bonusi = request.Bonusi;
            plata.Otkazi = request.Otkazi;
            plata.Period = request.Period;
            plata.Napomene = request.Napomene;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PlataExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/Plata/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> DeletePlata(int id)
        {
            var plata = await _context.Plate.FindAsync(id);
            if (plata == null)
            {
                return NotFound();
            }

            _context.Plate.Remove(plata);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/Plata/zaposleni/5
        [HttpGet("zaposleni/{zaposleniId}")]
        [Authorize(Roles = "SuperAdmin,HRManager,Administrator")]
        public async Task<ActionResult<IEnumerable<Plata>>> GetPlateZaZaposlenog(int zaposleniId)
        {
            var plate = await _context.Plate
                .Where(p => p.ZaposleniId == zaposleniId)
                .ToListAsync();

            return plate;
        }

        // GET: api/Plata/period/{period}
        [HttpGet("period/{period}")]
        [Authorize(Roles = "SuperAdmin,HRManager,Administrator")]
        public async Task<ActionResult<IEnumerable<Plata>>> GetPlateZaPeriod(string period)
        {
            var plate = await _context.Plate
                .Where(p => p.Period == period)
                .ToListAsync();

            return plate;
        }

        // GET: api/Plata/zaposleni/{zaposleniId}/period/{period}
        [HttpGet("zaposleni/{zaposleniId}/period/{period}")]
        [Authorize(Roles = "SuperAdmin,HRManager,Administrator")]
        public async Task<ActionResult<Plata>> GetPlataZaZaposlenogIPeriod(int zaposleniId, string period)
        {
            var plata = await _context.Plate
                .FirstOrDefaultAsync(p => p.ZaposleniId == zaposleniId && p.Period == period);

            if (plata == null)
            {
                return NotFound();
            }

            return plata;
        }

        // GET: api/Plata/report/period/{period}
        [HttpGet("report/period/{period}")]
        [Authorize(Roles = "SuperAdmin,HRManager")]
        public async Task<ActionResult<object>> GetPlateReport(string period)
        {
            var plate = await _context.Plate
                .Where(p => p.Period == period)
                .ToListAsync();

            var report = new
            {
                Period = period,
                UkupnoZaposlenih = plate.Count,
                UkupnaOsnovnaPlata = plate.Sum(p => p.Osnovna),
                UkupniBonusi = plate.Sum(p => p.Bonusi),
                UkupniOtkazi = plate.Sum(p => p.Otkazi),
                UkupnaNetoPlata = plate.Sum(p => p.Neto),
                ProsecnaPlata = plate.Any() ? plate.Average(p => p.Neto) : 0
            };

            return report;
        }

        private bool PlataExists(int id)
        {
            return _context.Plate.Any(e => e.Id == id);
        }
    }
}