using Microsoft.AspNetCore.Mvc;
using Hogwarts.Models;
using System.Collections.Generic;
using System.Linq;

// API kontroler za plate
[ApiController]
[Route("api/[controller]")]
public class PlataController : ControllerBase
{
    private static List<Plata> Plate = new(); // Korisnićemo lokalnu listu za test

    // GET sve
    [HttpGet]
    public ActionResult<IEnumerable<Plata>> Get()
    {
        return Ok(Plate);
    }

    // GET po ID
    [HttpGet("{id}")]
    public ActionResult<Plata> Get(int id)
    {
        var plata = Plate.FirstOrDefault(p => p.Id == id);
        if (plata == null)
            return NotFound();
        return Ok(plata);
    }

    // POST nova plata
    [HttpPost]
    public ActionResult<Plata> KreirajPlatu([FromBody] Plata novaPlata)
    {
        novaPlata.Id = Plate.Count > 0 ? Plate.Max(p => p.Id) + 1 : 1;
        Plate.Add(novaPlata);
        return CreatedAtAction(nameof(Get), new { id = novaPlata.Id }, novaPlata);
    }

    // PUT ažuriranje
    [HttpPut("{id}")]
    public IActionResult AzurirajPlatu(int id, [FromBody] Plata plataZaAzuriranje)
    {
        var existing = Plate.FirstOrDefault(p => p.Id == id);
        if (existing == null)
            return NotFound();

        // Ažuriraj sve polja
        existing.ZaposleniId = plataZaAzuriranje.ZaposleniId;
        existing.Osnovna = plataZaAzuriranje.Osnovna;
        existing.Bonusi = plataZaAzuriranje.Bonusi;
        existing.Otkazi = plataZaAzuriranje.Otkazi;
        existing.Period = plataZaAzuriranje.Period;
        existing.Neto = plataZaAzuriranje.Neto;

        return NoContent();
    }

    // DELETE plata
    [HttpDelete("{id}")]
    public IActionResult ObrisiPlatu(int id)
    {
        var existing = Plate.FirstOrDefault(p => p.Id == id);
        if (existing == null)
            return NotFound();

        Plate.Remove(existing);
        return NoContent();
    }
}
