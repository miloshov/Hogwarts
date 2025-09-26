using Microsoft.AspNetCore.Mvc;
using Hogwarts.Models;
using System.Collections.Generic;
using System.Linq;

[ApiController]
[Route("api/[controller]")]
public class ZahtevZaOdobrenjeController : ControllerBase
{
    private static List<ZahtevZaOdobrenje> Zahtevi = new();

    // Kreira novi zahtev
    [HttpPost]
    public IActionResult KreirajZahtev([FromBody] ZahtevZaOdobrenje zahtev)
    {
        zahtev.Id = Zahtevi.Count > 0 ? Zahtevi.Max(z => z.Id) + 1 : 1;
        Zahtevi.Add(zahtev);

        // jednostavna notifikacija (log poruka na serveru)
        Console.WriteLine($"Novi zahtev ID {zahtev.Id} za zaposlenog ID {zahtev.ZaposleniId} je kreiran.");

        return CreatedAtAction(nameof(Get), new { id = zahtev.Id }, zahtev);
    }

    // Odobrava zahtev
    [HttpPut("{id}/odobri")]
    public IActionResult Odobri(int id)
    {
        var zahtev = Zahtevi.FirstOrDefault(z => z.Id == id);
        if (zahtev == null)
            return NotFound();

        zahtev.Status = "Odobreno";

        // jednostavna notifikacija
        Console.WriteLine($"Zahtev ID {zahtev.Id} odobren.");

        return NoContent();
    }

    // Odbija zahtev
    [HttpPut("{id}/odbij")]
    public IActionResult Odbij(int id)
    {
        var zahtev = Zahtevi.FirstOrDefault(z => z.Id == id);
        if (zahtev == null)
            return NotFound();

        zahtev.Status = "Odbijeno";

        // jednostavna notifikacija
        Console.WriteLine($"Zahtev ID {zahtev.Id} odbijen.");

        return NoContent();
    }

    // Pregled svih zahteva
    [HttpGet]
    public ActionResult<IEnumerable<ZahtevZaOdobrenje>> Get()
    {
        return Ok(Zahtevi);
    }
}
