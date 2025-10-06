using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Hogwarts.Data;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Sve akcije zahtevaju autentifikaciju
public class DashboardController : ControllerBase
{
    private readonly HogwartsContext _context;

    public DashboardController(HogwartsContext context)
    {
        _context = context;
    }

    // GET: api/dashboard/statistics
    [HttpGet("statistics")]
    [Authorize(Roles = "SuperAdmin,HRManager,TeamLead")]
    public async Task<ActionResult<object>> GetStatistics()
    {
        try
        {
            var ukupnoZaposlenih = await _context.Zaposleni.CountAsync(z => z.IsActive);
            var ukupnoOdseka = await _context.Odseci.CountAsync(o => o.IsActive);
            
            // ProseÄna plata iz najnovijih plata po zaposlenom
            var najnovijePlate = await _context.Plate
                .GroupBy(p => p.ZaposleniId)
                .Select(g => g.OrderByDescending(p => p.DatumKreiranja).First())
                .ToListAsync();
            
            var prosecnaPlata = najnovijePlate.Any() ? Math.Round(najnovijePlate.Average(p => p.Neto), 2) : 0;
            var ukupneMesecnePlate = najnovijePlate.Sum(p => p.Neto);

            var aktivniZahtevi = await _context.ZahteviZaOdmor.CountAsync(z => z.Status == "Na_Cekanju");
            var odobrenihZahteva = await _context.ZahteviZaOdmor.CountAsync(z => z.Status == "Odobren");
            var odbacenihZahteva = await _context.ZahteviZaOdmor.CountAsync(z => z.Status == "Odbijen");

            // Najnoviji zaposleni (poslednji mesec)
            var poslednjihMesecDana = DateTime.UtcNow.AddDays(-30);
            var noviZaposleni = await _context.Zaposleni
                .CountAsync(z => z.DatumZaposlenja >= poslednjihMesecDana && z.IsActive);

            return Ok(new
            {
                ukupnoZaposlenih,
                ukupnoOdseka,
                prosecnaPlata,
                ukupneMesecnePlate = Math.Round(ukupneMesecnePlate, 2),
                aktivniZahtevi,
                odobrenihZahteva,
                odbacenihZahteva,
                noviZaposleni
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"GreÅ¡ka pri dobijanju statistika: {ex.Message}");
        }
    }

    // GET: api/dashboard/recent-activity
    [HttpGet("recent-activity")]
    [Authorize(Roles = "SuperAdmin,HRManager,TeamLead")]
    public async Task<ActionResult<object>> GetRecentActivity()
    {
        try
        {
            var poslednjePlate = await _context.Plate
                .Include(p => p.Zaposleni)
                .OrderByDescending(p => p.DatumKreiranja)
                .Take(5)
                .Select(p => new
                {
                    id = p.Id,
                    tip = "Plata",
                    naslov = $"Plata za {p.Zaposleni!.PunoIme}",
                    opis = $"Period: {p.Period} - Neto: {p.Neto:N0} RSD",
                    datum = p.DatumKreiranja,
                    zaposleniId = p.ZaposleniId,
                    zaposleni = p.Zaposleni!.PunoIme
                })
                .ToListAsync();

            var poslednjiZahtevi = await _context.ZahteviZaOdmor
                .Include(z => z.Zaposleni)
                .OrderByDescending(z => z.DatumZahteva)
                .Take(5)
                .Select(z => new
                {
                    id = z.Id,
                    tip = "Zahtev za odmor",
                    naslov = $"Zahtev - {z.Zaposleni!.PunoIme}",
                    opis = $"Tip: {z.TipOdmora} - Status: {GetStatusText(z.Status)}",
                    datum = z.DatumZahteva,
                    zaposleniId = z.ZaposleniId,
                    zaposleni = z.Zaposleni!.PunoIme
                })
                .ToListAsync();

            var noviZaposleni = await _context.Zaposleni
                .Include(z => z.Odsek)
                .Where(z => z.DatumZaposlenja >= DateTime.UtcNow.AddDays(-30) && z.IsActive)
                .OrderByDescending(z => z.DatumZaposlenja)
                .Take(5)
                .Select(z => new
                {
                    id = z.Id,
                    tip = "Novi zaposleni",
                    naslov = $"ZapoÅ¡ljavanje - {z.PunoIme}",
                    opis = $"Pozicija: {z.Pozicija} - Odsek: {(z.Odsek != null ? z.Odsek.Naziv : "Nedodeljen")}",
                    datum = z.DatumZaposlenja,
                    zaposleniId = z.Id,
                    zaposleni = z.PunoIme
                })
                .ToListAsync();

            // Kombinuj sve aktivnosti i sortiraj po datumu
            var aktivnosti = new List<object>();
            aktivnosti.AddRange(poslednjePlate);
            aktivnosti.AddRange(poslednjiZahtevi);
            aktivnosti.AddRange(noviZaposleni);

            var sortiraneAktivnosti = aktivnosti
                .OrderByDescending(a => ((dynamic)a).datum)
                .Take(10)
                .ToList();

            return Ok(sortiraneAktivnosti);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"GreÅ¡ka pri dobijanju nedavnih aktivnosti: {ex.Message}");
        }
    }

    // GET: api/dashboard/charts-data
    [HttpGet("charts-data")]
    [Authorize(Roles = "SuperAdmin,HRManager,TeamLead")]
    public async Task<ActionResult<object>> GetChartsData()
    {
        try
        {
            // Distribucija zaposlenih po odsecima
            var zaposleniPoOdsecima = await _context.Zaposleni
                .Include(z => z.Odsek)
                .Where(z => z.IsActive)
                .GroupBy(z => z.Odsek != null ? z.Odsek.Naziv : "Nedodeljen")
                .Select(g => new
                {
                    naziv = g.Key,
                    brojZaposlenih = g.Count()
                })
                .OrderByDescending(x => x.brojZaposlenih)
                .ToListAsync();

            // MeseÄne statistike za poslednje 6 meseci
            var poslednjih6Meseci = Enumerable.Range(0, 6)
                .Select(i => DateTime.UtcNow.AddMonths(-i))
                .OrderBy(d => d)
                .ToList();

            var mesecneStatistike = new List<object>();
            foreach (var mesec in poslednjih6Meseci)
            {
                var pocetakMeseca = new DateTime(mesec.Year, mesec.Month, 1);
                var krajMeseca = pocetakMeseca.AddMonths(1).AddDays(-1);
                var nazivMeseca = mesec.ToString("MMM yyyy");

                var plateUMesecu = await _context.Plate
                    .Where(p => p.DatumKreiranja >= pocetakMeseca && p.DatumKreiranja <= krajMeseca)
                    .SumAsync(p => p.Neto);

                var zahteviUMesecu = await _context.ZahteviZaOdmor
                    .CountAsync(z => z.DatumZahteva >= pocetakMeseca && z.DatumZahteva <= krajMeseca);

                var noviZaposleniUMesecu = await _context.Zaposleni
                    .CountAsync(z => z.DatumZaposlenja >= pocetakMeseca && z.DatumZaposlenja <= krajMeseca && z.IsActive);

                mesecneStatistike.Add(new
                {
                    mesec = nazivMeseca,
                    ukupnePlate = Math.Round(plateUMesecu, 2),
                    brojZahteva = zahteviUMesecu,
                    noviZaposleni = noviZaposleniUMesecu
                });
            }

            // Distribucija plata
            var najnovijePlate = await _context.Plate
                .GroupBy(p => p.ZaposleniId)
                .Select(g => g.OrderByDescending(p => p.DatumKreiranja).First())
                .Select(p => p.Neto)
                .ToListAsync();

            var platneGrupe = new List<object>
            {
                new { grupa = "Do 50.000", broj = najnovijePlate.Count(p => p < 50000) },
                new { grupa = "50.000 - 75.000", broj = najnovijePlate.Count(p => p >= 50000 && p < 75000) },
                new { grupa = "75.000 - 100.000", broj = najnovijePlate.Count(p => p >= 75000 && p < 100000) },
                new { grupa = "100.000 - 150.000", broj = najnovijePlate.Count(p => p >= 100000 && p < 150000) },
                new { grupa = "Preko 150.000", broj = najnovijePlate.Count(p => p >= 150000) }
            };

            return Ok(new
            {
                zaposleniPoOdsecima,
                mesecneStatistike,
                platneGrupe
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"GreÅ¡ka pri dobijanju podataka za grafikone: {ex.Message}");
        }
    }

    private static string GetStatusText(string status)
    {
        return status switch
        {
            "Na_Cekanju" => "Na Äekanju",
            "Odobren" => "Odobren",
            "Odbijen" => "Odbijen",
            _ => status
        };
    }
}