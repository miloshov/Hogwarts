using Microsoft.AspNetCore.Mvc;
using Hogwarts.Models;
using System.Collections.Generic;
using System.Linq;

// Ovo će upravljati pretplatama i slati push poruke
[ApiController]
[Route("api/[controller]")]
public class PushController : ControllerBase
{
    private static List<PushSubscription> Pretplate = new();

    // Primanje nove pretplate od frontenda
    [HttpPost("subscribe")]
    public IActionResult Subscribe([FromBody] PushSubscription subscription)
    {
        var existing = Pretplate.FirstOrDefault(p => p.Endpoint == subscription.Endpoint);
        if (existing != null)
        {
            return Ok("Pretplata već postoji");
        }
        subscription.Id = Pretplate.Count > 0 ? Pretplate.Max(p => p.Id) + 1 : 1;
        Pretplate.Add(subscription);
        return Ok("Pretplata spremljena");
    }

    // Slanje test push poruke svim pretplatnicima
    [HttpPost("notify")]
    public IActionResult Notify([FromBody] string message)
    {
        // Ovdje dodajte kod za slanje push poruke svim pretplatnicima
        // Za sada, samo simulacija
        foreach (var pretplata in Pretplate)
        {
            // TODO: Ovde treba poslati push zahtev
            System.Console.WriteLine($"Slanje poruke: {message} Pretplatnik Endpoint: {pretplata.Endpoint}");
        }
        return Ok("Poruka poslana");
    }
}
