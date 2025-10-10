using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Hogwarts.Models.DTOs;
using Hogwarts.Services;
using System.Security.Claims;

namespace Hogwarts.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Svi endpoint-i zahtevaju autentifikaciju
    public class InventarController : ControllerBase
    {
        private readonly IInventarService _inventarService;

        public InventarController(IInventarService inventarService)
        {
            _inventarService = inventarService;
        }

        // GET: api/inventar
        [HttpGet]
        public async Task<ActionResult<InventarPagedResult>> GetInventar([FromQuery] InventarFilterDto filter)
        {
            try
            {
                var result = await _inventarService.GetInventarPaginatedAsync(filter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri učitavanju inventara: {ex.Message}");
            }
        }

        // GET: api/inventar/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<InventarResponseDto>> GetInventarById(int id)
        {
            try
            {
                var inventar = await _inventarService.GetInventarByIdAsync(id);
                if (inventar == null)
                {
                    return NotFound($"Inventar stavka sa ID {id} nije pronađena");
                }

                return Ok(inventar);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri učitavanju inventara: {ex.Message}");
            }
        }

        // POST: api/inventar
        [HttpPost]
        [Authorize(Roles = "SuperAdmin,Admin,HR")] // Samo određene uloge mogu kreirati
        public async Task<ActionResult<InventarResponseDto>> CreateInventar([FromBody] CreateInventarDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var korisnik = GetCurrentUser();
                var inventar = await _inventarService.CreateInventarAsync(dto, korisnik);

                return CreatedAtAction(nameof(GetInventarById), new { id = inventar.Id }, inventar);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri kreiranju inventara: {ex.Message}");
            }
        }

        // PUT: api/inventar/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin,Admin,HR")]
        public async Task<ActionResult<InventarResponseDto>> UpdateInventar(int id, [FromBody] UpdateInventarDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var korisnik = GetCurrentUser();
                var inventar = await _inventarService.UpdateInventarAsync(id, dto, korisnik);

                return Ok(inventar);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri ažuriranju inventara: {ex.Message}");
            }
        }

        // DELETE: api/inventar/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> DeleteInventar(int id)
        {
            try
            {
                var success = await _inventarService.DeleteInventarAsync(id);
                if (!success)
                {
                    return NotFound($"Inventar stavka sa ID {id} nije pronađena");
                }

                return Ok(new { message = "Inventar stavka je uspešno obrisana" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri brisanju inventara: {ex.Message}");
            }
        }

        // POST: api/inventar/{id}/dodeli
        [HttpPost("{id}/dodeli")]
        [Authorize(Roles = "SuperAdmin,Admin,HR")]
        public async Task<IActionResult> DodelijInventar(int id, [FromBody] DodelijInventarDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var korisnik = GetCurrentUser();
                var success = await _inventarService.DodelijInventarAsync(id, dto, korisnik);
                
                if (!success)
                {
                    return BadRequest("Nije moguće dodeliti inventar. Proverite da li inventar i zaposleni postoje.");
                }

                return Ok(new { message = "Inventar je uspešno dodeljen zaposlenom" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri dodeljivanju inventara: {ex.Message}");
            }
        }

        // POST: api/inventar/{id}/vrati
        [HttpPost("{id}/vrati")]
        [Authorize(Roles = "SuperAdmin,Admin,HR")]
        public async Task<IActionResult> VratiInventar(int id, [FromBody] VratiInventarDto dto)
        {
            try
            {
                var korisnik = GetCurrentUser();
                var success = await _inventarService.VratiInventarAsync(id, dto, korisnik);
                
                if (!success)
                {
                    return BadRequest("Nije moguće vratiti inventar. Proverite da li je inventar trenutno dodeljen.");
                }

                return Ok(new { message = "Inventar je uspešno vraćen" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri vraćanju inventara: {ex.Message}");
            }
        }

        // GET: api/inventar/statistike
        [HttpGet("statistike")]
        public async Task<ActionResult<InventarStatistikeDto>> GetStatistike()
        {
            try
            {
                var statistike = await _inventarService.GetStatistikeAsync();
                return Ok(statistike);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri učitavanju statistika: {ex.Message}");
            }
        }

        // GET: api/inventar/{id}/qr-code
        [HttpGet("{id}/qr-code")]
        public async Task<IActionResult> GetQRCode(int id)
        {
            try
            {
                var qrCode = await _inventarService.GenerateQRCodeAsync(id);
                return File(qrCode, "image/png", $"inventar-{id}-qr.png");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri generisanju QR koda: {ex.Message}");
            }
        }

        // GET: api/inventar/kategorije
        [HttpGet("kategorije")]
        public async Task<ActionResult<List<string>>> GetKategorije()
        {
            try
            {
                var kategorije = await _inventarService.GetKategorije();
                return Ok(kategorije);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri učitavanju kategorija: {ex.Message}");
            }
        }

        // GET: api/inventar/lokacije
        [HttpGet("lokacije")]
        public async Task<ActionResult<List<string>>> GetLokacije()
        {
            try
            {
                var lokacije = await _inventarService.GetLokacije();
                return Ok(lokacije);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri učitavanju lokacija: {ex.Message}");
            }
        }

        // GET: api/inventar/zaposleni/{zaposleniId}
        [HttpGet("zaposleni/{zaposleniId}")]
        public async Task<ActionResult<List<InventarResponseDto>>> GetInventarByZaposleni(int zaposleniId)
        {
            try
            {
                var inventar = await _inventarService.GetInventarByZaposleniAsync(zaposleniId);
                return Ok(inventar);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri učitavanju inventara zaposlenog: {ex.Message}");
            }
        }

        // GET: api/inventar/stanja
        [HttpGet("stanja")]
        public ActionResult<List<string>> GetStanja()
        {
            try
            {
                var stanja = new List<string>
                {
                    "Novo",
                    "Korisceno", 
                    "Pokvareno",
                    "Izgubljeno",
                    "Otpisano"
                };
                return Ok(stanja);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri učitavanju stanja: {ex.Message}");
            }
        }

        // Helper metoda za dobijanje trenutnog korisnika
        private string GetCurrentUser()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity != null)
            {
                var userClaim = identity.FindFirst("unique_name") ?? 
                               identity.FindFirst("email") ?? 
                               identity.FindFirst(ClaimTypes.Name);
                return userClaim?.Value ?? "System";
            }
            return "System";
        }
    }
}