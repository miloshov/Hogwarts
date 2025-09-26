namespace Hogwarts.Models
{
    // Model zahteva za odobrenje (godišnji odmor, poslovno putovanje, itd.)
    public class ZahtevZaOdobrenje
    {
        public int Id { get; set; }                         // ID zahteva
        public int ZaposleniId { get; set; }                // ID zaposlenog koji traži odmor
        public DateTime DatumOd { get; set; }               // Datum početka
        public DateTime DatumDo { get; set; }               // Datum završetka
        public string Status { get; set; } = "Na čekanju"; // Status: na čekanju, odobreno, odbijeno
        public string Razlog { get; set; } = string.Empty; // Razlog zahteva
    }
}
