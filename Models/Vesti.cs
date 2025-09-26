namespace Hogwarts.Models
{
    public class Vesti
    {
        public int Id { get; set; }
        public string Naslov { get; set; } = string.Empty;
        public string Tekst { get; set; } = string.Empty;
        public DateTime Datum { get; set; } = DateTime.Now;
    }
}
