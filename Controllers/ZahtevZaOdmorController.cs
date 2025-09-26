using Microsoft.AspNetCore.Mvc;
using Hogwarts.Models;
using System.Collections.Generic;
using System.Linq;

// Za sada je to samo virtualna lista
[ApiController]
[Route("api/[controller]")]
public class ZahtevZaOdmorController : ControllerBase
{
    private static List<ZahtevZaOdmor> Zahtevi = new();

    // GET svi zahtevi
    [HttpGet]
    public ActionResult<IEnumerable<ZahtevZaOdmor>> Get()
    {
        return Ok(Zahtevi);
    }

    // POST novi zahtev
    [HttpPost]
    public ActionResult<ZahtevZaOdmor> Post([FromBody] ZahtevZaOdmor zahtev)
    {
        zahtev.Id = Zahtevi.Count > 0 ? Zahtevi.Max(z => z.Id) + 1 : 1;
        Zahtevi.Add(zahtev);
        return CreatedAtAction(nameof(Get), new { id = zahtev.Id }, zahtev);
    }

    // Odobri zahtev
    [HttpPut("{id}/odobri")]
    public IActionResult Odobri(int id)
    {
        var zahtev = Zahtevi.FirstOrDefault(z => z.Id == id);
        if (zahtev == null)
            return NotFound();

        zahtev.Status = "Odobreno";
        return NoContent();
    }

    // Odbij zahtev
    [HttpPut("{id}/odbij")]
    public IActionResult Odbij(int id)
    {
        var zahtev = Zahtevi.FirstOrDefault(z => z.Id == id);
        if (zahtev == null)
            return NotFound();

        zahtev.Status = "Odbijeno";
        return NoContent();
    }
}
