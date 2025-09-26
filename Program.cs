using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore; // EF Core
using Hogwarts.Data;               // Data sloj
using Hogwarts.Models;             // Modeli
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Konfiguracija EF Core da koristi PostgreSQL bazu
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Registruj kontrolere
builder.Services.AddControllers();

// Ako želiš Swagger za testiranje API-ja u razvojnom okruženju
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
}

// Konfiguracija JWT autentifikacije (pre `Build()`)
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("TvojaJakaSifraZaPotpisivanjeJWT"))
    };
});

// Izgradnja aplikacije
var app = builder.Build();

// Middleware za razradu i Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Ovaj red je važan, omogućava autentifikaciju pre autorizacije
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
