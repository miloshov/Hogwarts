namespace Hogwarts.Models
{
    // Klasa koja predstavlja podatke o zaposlenom u HR sistemu
    public class Zaposleni
    {
        // Jedinstveni identifikator zaposlenog
        public int Id { get; set; } 

        // Osobni podaci
        public string Ime { get; set; } = string.Empty; // Ime zaposlenog
        public string Prezime { get; set; } = string.Empty; // Prezime zaposlenog
        public string Email { get; set; } = string.Empty; // Kontakt email
        public string Pozicija { get; set; } = string.Empty; // Pozicija ili odeljenje

        // Datum kada je zaposleni započeo radni odnos
        public DateTime DatumZaposlenja { get; set; }

        // Dodatni podaci o zaposlenom
        public DateTime DatumRodjenja { get; set; } // Datum rođenja
        public string ImeOca { get; set; } = string.Empty; // Ime oca
        public string JMBG { get; set; } = string.Empty; // Jedinstveni Matični Broj Građana
        public string Adresa { get; set; } = string.Empty; // Stalna adresa
        public string BrojTelefon { get; set; } = string.Empty; // Kontakt telefon
    }
}
