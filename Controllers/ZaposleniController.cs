using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Hogwarts.Data;
using Hogwarts.Models;
using Hogwarts.DTOs;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ZaposleniController : ControllerBase
{
    private readonly HogwartsContext _context;
    private readonly IWebHostEnvironment _environment;

    public ZaposleniController(HogwartsContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    // 🆕 GET: api/zaposleni/dropdown - OPTIMIZOVAN ZA DROPDOWN MENI
    [HttpGet("dropdown")]
    [Authorize(Roles = "SuperAdmin,HRManager,TeamLead")]
    public async Task<ActionResult<IEnumerable<object>>> GetDropdown()
    {
        try
        {
            var zaposleni = await _context.Zaposleni
                .Where(z => z.IsActive)
                .OrderBy(z => z.Ime)
                .ThenBy(z => z.Prezime)
                .Select(z => new
                {
                    id = z.Id,
                    ime = z.Ime,
                    prezime = z.Prezime,
                    punoIme = z.PunoIme,
                    pozicija = z.Pozicija ?? "N/A"
                })
                .ToListAsync();

            // DIREKTAN NIZ - bez paginacije, bez wrapping objekta
            return Ok(zaposleni);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GET zaposleni dropdown: {ex.Message}");
            return StatusCode(500, $"Greška pri dobijanju dropdown liste: {ex.Message}");
        }
    }

    // GET: api/zaposleni
    [HttpGet]
    [Authorize(Roles = "SuperAdmin,HRManager,TeamLead")]
    public async Task<ActionResult<object>> Get(
        int page = 1, 
        int pageSize = 10, 
        string search = "", 
        string sortBy = "ime", 
        bool ascending = true)
    {
        try
        {
            var query = _context.Zaposleni
                .Include(z => z.Odsek)
                .Where(z => z.IsActive);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(z => 
                    z.Ime.Contains(search) ||
                    z.Prezime.Contains(search) ||
                    z.Email.Contains(search) ||
                    (z.Pozicija != null && z.Pozicija.Contains(search)));
            }

            query = sortBy.ToLower() switch
            {
                "prezime" => ascending ? query.OrderBy(z => z.Prezime) : query.OrderByDescending(z => z.Prezime),
                "email" => ascending ? query.OrderBy(z => z.Email) : query.OrderByDescending(z => z.Email),
                "pozicija" => ascending ? query.OrderBy(z => z.Pozicija) : query.OrderByDescending(z => z.Pozicija),
                "datumzaposlenja" => ascending ? query.OrderBy(z => z.DatumZaposlenja) : query.OrderByDescending(z => z.DatumZaposlenja),
                _ => ascending ? query.OrderBy(z => z.Ime) : query.OrderByDescending(z => z.Ime)
            };

            var totalCount = await query.CountAsync();
            var zaposleni = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(z => new
                {
                    z.Id,
                    z.Ime,
                    z.Prezime,
                    z.Email,
                    z.Pozicija,
                    z.DatumZaposlenja,
                    z.DatumRodjenja,
                    z.ImeOca,
                    z.JMBG,
                    z.Adresa,
                    z.BrojTelefon,
                    z.PunoIme,
                    z.Godine,
                    z.OdsekId,
                    OdsekNaziv = z.Odsek != null ? z.Odsek.Naziv : null,
                    z.IsActive,
                    z.DatumKreiranja,
                    z.ProfileImageUrl,
                    z.Pol,
                    z.AvatarUrl
                })
                .ToListAsync();

            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            return Ok(new
            {
                data = zaposleni,
                pagination = new
                {
                    currentPage = page,
                    pageSize,
                    totalCount,
                    totalPages,
                    hasNext = page < totalPages,
                    hasPrevious = page > 1
                }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GET zaposleni: {ex.Message}");
            return StatusCode(500, $"Greška pri dobijanju zaposlenih: {ex.Message}");
        }
    }

    // GET: api/zaposleni/all (without pagination for dropdowns)
    [HttpGet("all")]
    [Authorize(Roles = "SuperAdmin,HRManager,TeamLead")]
    public async Task<ActionResult<IEnumerable<object>>> GetAll()
    {
        try
        {
            var zaposleni = await _context.Zaposleni
                .Include(z => z.Odsek)
                .Where(z => z.IsActive)
                .Select(z => new
                {
                    z.Id,
                    z.Ime,
                    z.Prezime,
                    z.Email,
                    z.Pozicija,
                    z.DatumZaposlenja,
                    z.DatumRodjenja,
                    z.ImeOca,
                    z.JMBG,
                    z.Adresa,
                    z.BrojTelefon,
                    z.PunoIme,
                    z.Godine,
                    z.OdsekId,
                    OdsekNaziv = z.Odsek != null ? z.Odsek.Naziv : null,
                    z.ProfileImageUrl,
                    z.Pol,
                    z.AvatarUrl
                })
                .ToListAsync();

            return Ok(zaposleni);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GET all zaposleni: {ex.Message}");
            return StatusCode(500, $"Greška pri dobijanju zaposlenih: {ex.Message}");
        }
    }

    // GET: api/zaposleni/5
    [HttpGet("{id}")]
    public async Task<ActionResult<ZaposleniDto>> Get(int id)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var currentUserRole = GetCurrentUserRole();
            var zaposleni = await _context.Zaposleni
                .Include(z => z.Odsek)
                .FirstOrDefaultAsync(z => z.Id == id);

            if (zaposleni == null)
                return NotFound("Zaposleni nije pronađen.");

            if (currentUserRole == UserRoles.Zaposleni)
            {
                var currentUser = await _context.Korisnici
                    .FirstOrDefaultAsync(k => k.Id == currentUserId);
                
                if (currentUser?.ZaposleniId != id)
                {
                    return Forbid("Nemate dozvolu za pristup ovim podacima.");
                }
            }

            var zaposleniDto = zaposleni.ToDto();
            return Ok(zaposleniDto);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GET zaposleni by id: {ex.Message}");
            return StatusCode(500, $"Greška pri dobijanju zaposlenog: {ex.Message}");
        }
    }

    // GET: api/zaposleni/moji-podaci
    [HttpGet("moji-podaci")]
    public async Task<ActionResult> GetMojiPodaci()
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var currentUser = await _context.Korisnici
                .Include(k => k.Zaposleni)
                .ThenInclude(z => z!.Odsek)
                .FirstOrDefaultAsync(k => k.Id == currentUserId);

            if (currentUser?.Zaposleni == null)
            {
                return NotFound("Podaci o zaposlenom nisu pronađeni.");
            }

            var zaposleniDto = currentUser.Zaposleni.ToDto();
            return Ok(zaposleniDto);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GET moji-podaci: {ex.Message}");
            return StatusCode(500, $"Greška pri dobijanju ličnih podataka: {ex.Message}");
        }
    }

    // POST: api/zaposleni/{id}/upload-image
    [HttpPost("{id}/upload-image")]
    [Authorize(Roles = "SuperAdmin,HRManager")]
    public async Task<ActionResult> UploadProfileImage(int id, IFormFile file)
    {
        try
        {
            var zaposleni = await _context.Zaposleni.FindAsync(id);
            if (zaposleni == null)
                return NotFound("Zaposleni nije pronađen.");

            if (file == null || file.Length == 0)
                return BadRequest("Fajl nije odabran.");

            var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
            if (!allowedTypes.Contains(file.ContentType.ToLower()))
                return BadRequest("Dozvoljeni su samo JPEG, PNG i GIF fajlovi.");

            if (file.Length > 5 * 1024 * 1024)
                return BadRequest("Fajl ne sme biti veći od 5MB.");

            var uploadsDir = Path.Combine(_environment.WebRootPath, "images", "profiles");
            if (!Directory.Exists(uploadsDir))
                Directory.CreateDirectory(uploadsDir);

            if (!string.IsNullOrEmpty(zaposleni.ProfileImageUrl))
            {
                var oldImagePath = Path.Combine(_environment.WebRootPath, zaposleni.ProfileImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
            }

            var fileExtension = Path.GetExtension(file.FileName);
            var fileName = $"profile_{id}_{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadsDir, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            zaposleni.ProfileImageUrl = $"/images/profiles/{fileName}";
            await _context.SaveChangesAsync();

            Console.WriteLine($"Profile image uploaded for employee {id}: {fileName}");
            return Ok(new { 
                message = "Slika je uspešno uploadovana.",
                imageUrl = zaposleni.ProfileImageUrl,
                avatarUrl = zaposleni.AvatarUrl
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error uploading profile image: {ex.Message}");
            return StatusCode(500, $"Greška pri uploadu slike: {ex.Message}");
        }
    }

    // DELETE: api/zaposleni/{id}/delete-image
    [HttpDelete("{id}/delete-image")]
    [Authorize(Roles = "SuperAdmin,HRManager")]
    public async Task<ActionResult> DeleteProfileImage(int id)
    {
        try
        {
            var zaposleni = await _context.Zaposleni.FindAsync(id);
            if (zaposleni == null)
                return NotFound("Zaposleni nije pronađen.");

            if (string.IsNullOrEmpty(zaposleni.ProfileImageUrl))
                return BadRequest("Zaposleni nema uploadovanu sliku.");

            var imagePath = Path.Combine(_environment.WebRootPath, zaposleni.ProfileImageUrl.TrimStart('/'));
            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }

            zaposleni.ProfileImageUrl = null;
            await _context.SaveChangesAsync();

            Console.WriteLine($"Profile image deleted for employee {id}");
            return Ok(new { 
                message = "Slika je uspešno obrisana.",
                avatarUrl = zaposleni.AvatarUrl
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting profile image: {ex.Message}");
            return StatusCode(500, $"Greška pri brisanju slike: {ex.Message}");
        }
    }

    // POST: api/zaposleni
    [HttpPost]
    [Authorize(Roles = "SuperAdmin,HRManager")]
    public async Task<ActionResult<Zaposleni>> Post([FromBody] Zaposleni noviZaposleni)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var postojeciZaposleni = await _context.Zaposleni
                .AnyAsync(z => z.Email == noviZaposleni.Email && z.IsActive);
            if (postojeciZaposleni)
            {
                return BadRequest("Zaposleni sa datim email-om već postoji.");
            }

            noviZaposleni.DatumKreiranja = DateTime.UtcNow;
            noviZaposleni.DatumZaposlenja = DateTime.SpecifyKind(noviZaposleni.DatumZaposlenja, DateTimeKind.Utc);
            noviZaposleni.DatumRodjenja = DateTime.SpecifyKind(noviZaposleni.DatumRodjenja, DateTimeKind.Utc);
            noviZaposleni.IsActive = true;

            _context.Zaposleni.Add(noviZaposleni);
            await _context.SaveChangesAsync();

            Console.WriteLine($"Uspešno dodat zaposleni: {noviZaposleni.Ime} {noviZaposleni.Prezime}");
            return CreatedAtAction(nameof(Get), new { id = noviZaposleni.Id }, noviZaposleni);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in POST zaposleni: {ex.Message}");
            Console.WriteLine($"StackTrace: {ex.StackTrace}");
            return StatusCode(500, $"Greška pri dodavanju zaposlenog: {ex.Message}");
        }
    }

    // PUT: api/zaposleni/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, [FromBody] Zaposleni azuriraniZaposleni)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = GetCurrentUserId();
            var currentUserRole = GetCurrentUserRole();
            var postojeciZaposleni = await _context.Zaposleni
                .FirstOrDefaultAsync(z => z.Id == id);

            if (postojeciZaposleni == null)
                return NotFound("Zaposleni nije pronađen.");

            if (currentUserRole == UserRoles.Zaposleni)
            {
                var currentUser = await _context.Korisnici
                    .FirstOrDefaultAsync(k => k.Id == currentUserId);
                
                if (currentUser?.ZaposleniId != id)
                {
                    return Forbid("Nemate dozvolu za izmenu ovih podataka.");
                }

                postojeciZaposleni.Email = azuriraniZaposleni.Email;
                postojeciZaposleni.Adresa = azuriraniZaposleni.Adresa;
                postojeciZaposleni.BrojTelefon = azuriraniZaposleni.BrojTelefon;
            }
            else if (currentUserRole == UserRoles.HRManager || currentUserRole == UserRoles.SuperAdmin)
            {
                postojeciZaposleni.Ime = azuriraniZaposleni.Ime;
                postojeciZaposleni.Prezime = azuriraniZaposleni.Prezime;
                postojeciZaposleni.Email = azuriraniZaposleni.Email;
                postojeciZaposleni.Pozicija = azuriraniZaposleni.Pozicija;
                
                postojeciZaposleni.DatumZaposlenja = DateTime.SpecifyKind(azuriraniZaposleni.DatumZaposlenja, DateTimeKind.Utc);
                postojeciZaposleni.DatumRodjenja = DateTime.SpecifyKind(azuriraniZaposleni.DatumRodjenja, DateTimeKind.Utc);
                
                postojeciZaposleni.ImeOca = azuriraniZaposleni.ImeOca;
                postojeciZaposleni.JMBG = azuriraniZaposleni.JMBG;
                postojeciZaposleni.Adresa = azuriraniZaposleni.Adresa;
                postojeciZaposleni.BrojTelefon = azuriraniZaposleni.BrojTelefon;
                postojeciZaposleni.OdsekId = azuriraniZaposleni.OdsekId;
                postojeciZaposleni.Pol = azuriraniZaposleni.Pol;
            }
            else
            {
                return Forbid("Nemate dozvolu za izmenu podataka.");
            }

            await _context.SaveChangesAsync();
            Console.WriteLine($"Uspešno ažuriran zaposleni: {postojeciZaposleni.Ime} {postojeciZaposleni.Prezime}");
            return NoContent();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in PUT zaposleni: {ex.Message}");
            Console.WriteLine($"StackTrace: {ex.StackTrace}");
            return StatusCode(500, $"Greška pri ažuriranju zaposlenog: {ex.Message}");
        }
    }

    // DELETE: api/zaposleni/5
    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin,HRManager")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var zaposleni = await _context.Zaposleni
                .FirstOrDefaultAsync(z => z.Id == id);
            if (zaposleni == null)
                return NotFound("Zaposleni nije pronađen.");

            zaposleni.IsActive = false;
            await _context.SaveChangesAsync();

            Console.WriteLine($"Zaposleni označen kao neaktivan: {zaposleni.Ime} {zaposleni.Prezime}");
            return NoContent();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in DELETE zaposleni: {ex.Message}");
            return StatusCode(500, $"Greška pri brisanju zaposlenog: {ex.Message}");
        }
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
    }

    private string GetCurrentUserRole()
    {
        var roleClaim = User.FindFirst(ClaimTypes.Role);
        return roleClaim?.Value ?? "";
    }
}