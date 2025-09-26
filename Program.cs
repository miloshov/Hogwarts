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

// Ako želite Swagger za testiranje API, potrebno je dodati
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Podesi JWT autentifikaciju
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
}); // Zatvaramo sve funkcije s pravom zatvarajućom zagradom

// Završavamo konfiguraciju i kreiramo aplikaciju
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseDeveloperExceptionPage();


app.UseAuthentication(); // Mora prvi
app.UseAuthorization();

app.MapControllers();

app.Run();
