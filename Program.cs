using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore; // Za EF Core
using Hogwarts.Data;               // Za našu bazu
using Hogwarts.Models;             // Za modele

var builder = WebApplication.CreateBuilder(args);

// Konfiguracija EF Core da koristi PostgreSQL bazu
builder.Services.AddDbContext<Hogwarts.Data.AppDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});


// #1: Ovde ubaci ovu liniju za bazu
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// #2: Registruj kontrolere
builder.Services.AddControllers();

// Ako želiš edžaj Swagger za testiranje API
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
}

// #3: Ostali middleware i konfiguracija
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers(); // To je obavezno, povezuje kontrolere na rute

app.Run();
