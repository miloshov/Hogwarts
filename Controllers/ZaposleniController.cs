using Microsoft.AspNetCore.Mvc;
using Hogwarts.Models; // Model za zaposlene

[ApiController]
[Route("api/[controller]")]
public class ZaposleniController : ControllerBase
{
    // Koristićemo statičku listu za priču - kasnije ćemo dodati bazu
    private static List<Zaposleni> zaposleniLista = new List<Zaposleni>();

    // GET: api/zaposleni
    [HttpGet]
    public ActionResult<IEnumerable<Zaposleni>> Get()
    {
        return Ok(zaposleniLista);
    }

    // GET: api/zaposleni/5
    [HttpGet("{id}")]
    public ActionResult<Zaposleni> Get(int id)
    {
        var zaposleni = zaposleniLista.FirstOrDefault(z => z.Id == id);
        if (zaposleni == null)
            return NotFound();
        return Ok(zaposleni);
    }

    // POST: api/zaposleni
    [HttpPost]
    public ActionResult<Zaposleni> Post([FromBody] Zaposleni noviZaposleni)
    {
        noviZaposleni.Id = zaposleniLista.Count > 0 ? zaposleniLista.Max(z => z.Id) + 1 : 1;
        zaposleniLista.Add(noviZaposleni);
        return CreatedAtAction(nameof(Get), new { id = noviZaposleni.Id }, noviZaposleni);
    }

    // PUT: api/zaposleni/5
    [HttpPut("{id}")]
    public IActionResult Put(int id, [FromBody] Zaposleni azuriraniZaposleni)
    {
        var postojećiZaposleni = zaposleniLista.FirstOrDefault(z => z.Id == id);
        if (postojećiZaposleni == null)
            return NotFound();

        // Azuriranje svih polja
        postojećiZaposleni.Ime = azuriraniZaposleni.Ime;
        postojećiZaposleni.Prezime = azuriraniZaposleni.Prezime;
        postojećiZaposleni.Email = azuriraniZaposleni.Email;
        postojećiZaposleni.Pozicija = azuriraniZaposleni.Pozicija;
        postojećiZaposleni.DatumZaposlenja = azuriraniZaposleni.DatumZaposlenja;
        postojećiZaposleni.DatumRodjenja = azuriraniZaposleni.DatumRodjenja;
        postojećiZaposleni.ImeOca = azuriraniZaposleni.ImeOca;
        postojećiZaposleni.JMBG = azuriraniZaposleni.JMBG;
        postojećiZaposleni.Adresa = azuriraniZaposleni.Adresa;
        postojećiZaposleni.BrojTelefon = azuriraniZaposleni.BrojTelefon;

        return NoContent();
    }

    // DELETE: api/zaposleni/5
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var zaposleni = zaposleniLista.FirstOrDefault(z => z.Id == id);
        if (zaposleni == null)
            return NotFound();

        zaposleniLista.Remove(zaposleni);
        return NoContent();
    }
}
