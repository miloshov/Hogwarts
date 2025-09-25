using Microsoft.EntityFrameworkCore;
using Hogwarts.Models; // Modeli

namespace Hogwarts.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Zaposleni> Zaposleni { get; set; }
    }
}
