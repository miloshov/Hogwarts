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
                    pozicija = z.PozicijaDisplay
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
    public async Task<ActionResult<object>> GetZaposleni(
        int page = 1, 
        int pageSize = 10, 
        string sortBy = "ime", 
        bool ascending = true, 
        string search = "", 
        int? odsekId = null)
    {
        try
        {
            var query = _context.Zaposleni
                .Include(z => z.Odsek)
                .Where(z => z.IsActive)
                .AsQueryable();

            // Department filter
            if (odsekId.HasValue)
            {
                query = query.Where(z => z.OdsekId == odsekId.Value);
            }

            // Search functionality
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(z => 
                    z.Ime.Contains(search) ||
                    z.Prezime.Contains(search) ||
                    z.Email.Contains(search) ||
                    z.PozicijaDisplay.Contains(search));
            }

            query = sortBy.ToLower() switch
            {
                "prezime" => ascending ? query.OrderBy(z => z.Prezime) : query.OrderByDescending(z => z.Prezime),
                "email" => ascending ? query.OrderBy(z => z.Email) : query.OrderByDescending(z => z.Email),
                "pozicija" => ascending ? query.OrderBy(z => z.PozicijaDisplay) : query.OrderByDescending(z => z.PozicijaDisplay),
                "datumzaposlenja" => ascending ? query.OrderBy(z => z.DatumZaposlenja) : query.OrderByDescending(z => z.DatumZaposlenja),
                _ => ascending ? query.OrderBy(z => z.Ime) : query.OrderByDescending(z => z.Ime)
            };

            var totalRecords = await query.CountAsync();

            var zaposleni = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(z => new
                {
                    z.Id,
                    z.Ime,
                    z.Prezime,
                    z.Email,
                    z.PozicijaDisplay,
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

            // Povrćamo paginated format sa svim potrebnim meta podacima
           return Ok(new
        {
            data = zaposleni,
            pagination = new
            {
                totalCount = totalRecords,
                totalPages = (int)Math.Ceiling((double)totalRecords / pageSize),
                currentPage = page,
                pageSize
            }
        });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GET zaposleni: {ex.Message}");
            return StatusCode(500, $"Greška pri dohvatanju zaposlenih: {ex.Message}");
        }
    }

    // GET: api/zaposleni/all (without pagination for dropdowns)
    [HttpGet("all")]
    [Authorize]
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
        Pozicija = z.PozicijaDisplay,  // ✅ DODANO
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
            Console.WriteLine($"Error in GET zaposleni/all: {ex.Message}");
            return StatusCode(500, $"Greška pri dobijanju svih zaposlenih: {ex.Message}");
        }
    }

    // GET: api/zaposleni/{id}
    [HttpGet("{id}")]
    [Authorize(Roles = "SuperAdmin,HRManager,TeamLead,Employee")]
    public async Task<ActionResult<ZaposleniDto>> GetZaposleni(int id)
    {
        try
        {
            // Proverava da li korisnik može da vidi ovog zaposlenog
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
            
            // Za Employee role, dozvoliti samo pregled svojih podataka
            if (currentUserRole == "Employee")
            {
                var currentUserZaposleniId = User.FindFirst("ZaposleniId")?.Value;
                if (currentUserZaposleniId != id.ToString())
                {
                    return Forbid("Nemate dozvolu za pregled podataka ovog zaposlenog");
                }
            }

            var zaposleni = await _context.Zaposleni
                .Include(z => z.Odsek)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (zaposleni == null)
            {
                return NotFound($"Zaposleni sa ID {id} nije pronađen");
            }

            var dto = zaposleni.ToDto();
            return Ok(dto);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GET zaposleni by ID: {ex.Message}");
            return StatusCode(500, $"Greška pri dobijanju zaposlenog: {ex.Message}");
        }
    }

    // POST: api/zaposleni
    [HttpPost]
    [Authorize(Roles = "SuperAdmin,HRManager")]
    public async Task<ActionResult<ZaposleniDto>> PostZaposleni([FromBody] CreateZaposleniDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Proveri da email već ne postoji
            if (await _context.Zaposleni.AnyAsync(z => z.Email == createDto.Email))
            {
                return Conflict("Zaposleni sa ovom email adresom već postoji");
            }

            // Proveri da JMBG već ne postoji (ako je unet)
            if (!string.IsNullOrEmpty(createDto.JMBG) && await _context.Zaposleni.AnyAsync(z => z.JMBG == createDto.JMBG))
            {
                return Conflict("Zaposleni sa ovim JMBG već postoji");
            }

            var zaposleni = createDto.ToEntity();
            _context.Zaposleni.Add(zaposleni);
            await _context.SaveChangesAsync();

            var dto = zaposleni.ToDto();
            return CreatedAtAction(nameof(GetZaposleni), new { id = zaposleni.Id }, dto);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in POST zaposleni: {ex.Message}");
            return StatusCode(500, $"Greška pri kreiranju zaposlenog: {ex.Message}");
        }
    }

    // PUT: api/zaposleni/{id}
    [HttpPut("{id}")]
    [Authorize(Roles = "SuperAdmin,HRManager")]
    public async Task<IActionResult> PutZaposleni(int id, [FromBody] CreateZaposleniDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var zaposleni = await _context.Zaposleni.FindAsync(id);
            if (zaposleni == null)
            {
                return NotFound($"Zaposleni sa ID {id} nije pronađen");
            }

            // Proveri da email već ne postoji (osim kod trenutnog zaposlenog)
            if (await _context.Zaposleni.AnyAsync(z => z.Email == updateDto.Email && z.Id != id))
            {
                return Conflict("Zaposleni sa ovom email adresom već postoji");
            }

            // Proveri da JMBG već ne postoji (osim kod trenutnog zaposlenog)
            if (!string.IsNullOrEmpty(updateDto.JMBG) && await _context.Zaposleni.AnyAsync(z => z.JMBG == updateDto.JMBG && z.Id != id))
            {
                return Conflict("Zaposleni sa ovim JMBG već postoji");
            }

            // Update from DTO
            zaposleni.UpdateFromDto(updateDto);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in PUT zaposleni: {ex.Message}");
            return StatusCode(500, $"Greška pri ažuriranju zaposlenog: {ex.Message}");
        }
    }

    // DELETE: api/zaposleni/{id}
    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> DeleteZaposleni(int id)
    {
        try
        {
            var zaposleni = await _context.Zaposleni.FindAsync(id);
            if (zaposleni == null)
            {
                return NotFound($"Zaposleni sa ID {id} nije pronađen");
            }

            // Soft delete - postavi IsActive = false umesto brisanja
            zaposleni.IsActive = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in DELETE zaposleni: {ex.Message}");
            return StatusCode(500, $"Greška pri brisanju zaposlenog: {ex.Message}");
        }
    }

    // 🆕 POST: api/zaposleni/{id}/upload-image - Upload profile image
    [HttpPost("{id}/upload-image")]
    [Authorize(Roles = "SuperAdmin,HRManager,Employee")]
    public async Task<IActionResult> UploadProfileImage(int id, IFormFile image)
    {
        try
        {
            // Proverava da li korisnik može da update-uje ovog zaposlenog
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (currentUserRole == "Employee")
            {
                var currentUserZaposleniId = User.FindFirst("ZaposleniId")?.Value;
                if (currentUserZaposleniId != id.ToString())
                {
                    return Forbid("Nemate dozvolu za update slike ovog zaposlenog");
                }
            }

            var zaposleni = await _context.Zaposleni.FindAsync(id);
            if (zaposleni == null)
            {
                return NotFound($"Zaposleni sa ID {id} nije pronađen");
            }

            if (image == null || image.Length == 0)
            {
                return BadRequest("Nisu uploaded file");
            }

            // Validacija file tipova
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var fileExtension = Path.GetExtension(image.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
            {
                return BadRequest("Dozvoljeni su samo JPG, PNG i GIF fajlovi");
            }

            // Validacija file veličine (5MB limit)
            if (image.Length > 5 * 1024 * 1024)
            {
                return BadRequest("Fajl ne sme biti veći od 5MB");
            }

            // Kreiraj uploads direktorij ako ne postoji
            var uploadsPath = Path.Combine(_environment.WebRootPath, "images", "profiles");
            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
            }

            // Generiši unique filename
            var fileName = $"{zaposleni.Id}_{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadsPath, fileName);

            // Obriši staru sliku ako postoji
            if (!string.IsNullOrEmpty(zaposleni.ProfileImageUrl))
            {
                var oldFileName = Path.GetFileName(zaposleni.ProfileImageUrl);
                var oldFilePath = Path.Combine(uploadsPath, oldFileName);
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }
            }

            // Save novi file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            // Update database
            zaposleni.ProfileImageUrl = $"/images/profiles/{fileName}";
            await _context.SaveChangesAsync();

            return Ok(new { 
                message = "Slika je uspešno uploaded", 
                imageUrl = zaposleni.ProfileImageUrl,
                avatarUrl = zaposleni.AvatarUrl 
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in upload profile image: {ex.Message}");
            return StatusCode(500, $"Greška pri upload-u slike: {ex.Message}");
        }
    }

    // 🆕 DELETE: api/zaposleni/{id}/delete-image - Obriši profile image
    [HttpDelete("{id}/delete-image")]
    [Authorize(Roles = "SuperAdmin,HRManager,Employee")]
    public async Task<IActionResult> DeleteProfileImage(int id)
    {
        try
        {
            // Proverava da li korisnik može da update-uje ovog zaposlenog
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (currentUserRole == "Employee")
            {
                var currentUserZaposleniId = User.FindFirst("ZaposleniId")?.Value;
                if (currentUserZaposleniId != id.ToString())
                {
                    return Forbid("Nemate dozvolu za brisanje slike ovog zaposlenog");
                }
            }

            var zaposleni = await _context.Zaposleni.FindAsync(id);
            if (zaposleni == null)
            {
                return NotFound($"Zaposleni sa ID {id} nije pronađen");
            }

            if (string.IsNullOrEmpty(zaposleni.ProfileImageUrl))
            {
                return BadRequest("Zaposleni nema uploaded sliku");
            }

            // Obriši fajl sa disk-a
            var fileName = Path.GetFileName(zaposleni.ProfileImageUrl);
            var uploadsPath = Path.Combine(_environment.WebRootPath, "images", "profiles");
            var filePath = Path.Combine(uploadsPath, fileName);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            // Update database
            zaposleni.ProfileImageUrl = null;
            await _context.SaveChangesAsync();

            return Ok(new { 
                message = "Slika je uspešno obrisana",
                avatarUrl = zaposleni.AvatarUrl 
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in delete profile image: {ex.Message}");
            return StatusCode(500, $"Greška pri brisanju slike: {ex.Message}");
        }
    }
}