using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginModel login)
    {
        // U praksi, ovde ide autentifikacija prema bazi
        if (login.UserName == "admin" && login.Password == "password")
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("TvojaJakaSifraZaPotpisivanjeJWT"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, login.UserName),
                new Claim(ClaimTypes.Role, "Admin") // ili "Zaposleni", "Menedžer"
            };

            var token = new JwtSecurityToken(
                issuer: null,
                audience: null,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            return Ok(new { Token = tokenString });
        }
        return Unauthorized();
    }
}

public class LoginModel
{
    public string UserName { get; set; } = string.Empty; // Obavezno
    public string Password { get; set; } = string.Empty; // Obavezno
}
