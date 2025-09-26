using Microsoft.AspNetCore.Mvc;
using Hogwarts.Models;
using System.Collections.Generic;
using System.Linq;

[ApiController]
[Route("api/[controller]")]
public class VestiController : ControllerBase
{
    private static List<Vesti> Vesti = new();

    [HttpGet]
    public ActionResult<IEnumerable<Vesti>> Get()
    {
        return Ok(Vesti);
    }

    [HttpGet("{id}")]
    public ActionResult<Vesti> Get(int id)
    {
        var vest = Vesti.FirstOrDefault(v => v.Id == id);
        if (vest == null)
            return NotFound();
        return Ok(vest);
    }

    [HttpPost]
    public ActionResult<Vesti> Post([FromBody] Vesti novaVest)
    {
        novaVest.Id = Vesti.Count > 0 ? Vesti.Max(v => v.Id) + 1 : 1;
        novaVest.Datum = DateTime.Now;
        Vesti.Add(novaVest);
        return CreatedAtAction(nameof(Get), new { id = novaVest.Id }, novaVest);
    }

    [HttpPut("{id}")]
    public IActionResult Put(int id, [FromBody] Vesti vestZaIzmenu)
    {
        var postojećaVesta = Vesti.FirstOrDefault(v => v.Id == id);
        if (postojećaVesta == null)
            return NotFound();

        postojećaVesta.Naslov = vestZaIzmenu.Naslov;
        postojećaVesta.Tekst = vestZaIzmenu.Tekst;
        return NoContent();
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var vest = Vesti.FirstOrDefault(v => v.Id == id);
        if (vest == null)
            return NotFound();

        Vesti.Remove(vest);
        return NoContent();
    }
}
