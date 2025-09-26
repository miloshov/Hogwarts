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

// Register kontrolere
builder.Services.AddControllers();

// Swagger (za razvojnu okolinu)
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
}

// JWT autentifikacija
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

// CORS politika za frontend na localhost:3000
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactOrigin",
        builder => builder
            .WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

// Završavamo konfiguraciju
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Dodajimo CORS pre autentifikacije
app.UseHttpsRedirection();
app.UseCors("AllowReactOrigin");  // OVO JE VAŽNO, da omogućimo pristup sa React frontend-a

app.UseDeveloperExceptionPage();

// Autentifikacija i autorizacija
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
