namespace Hogwarts.Models
{
    // Klasa koja opisuje zahtev za odmor
    public class ZahtevZaOdmor
    {
        public int Id { get; set; } // Jedinstveni identifikator zahteva

        public int ZaposleniId { get; set; } // ID zaposlenog koji traži odmor
        public DateTime DatumOd { get; set; } // Datum početka odmore
        public DateTime DatumDo { get; set; } // Datum završetka odmore

        public string Status { get; set; } = "Na čekanju"; // Status zahteva: Na čekanju, Odobreno, Odbijeno
        public string Razlog { get; set; } = string.Empty; // Razlog zahteva
    }
}
