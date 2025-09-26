namespace Hogwarts.Models
{
    public class Plata
    {
        public int Id { get; set; }
        public int ZaposleniId { get; set; } // ID zaposlenog
        public decimal Osnovna { get; set; } // Osnovna plata
        public decimal Bonusi { get; set; } // Bonus
        public decimal Otkazi { get; set; } // Otkazi
        public string Period { get; set; } = string.Empty; // Period isplate, npr. mesec i godina
        public decimal Neto { get; set; } // Neto plata nakon odbitaka
    }
}
