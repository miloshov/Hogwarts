using Hogwarts.Models;

namespace Hogwarts.DTOs
{
    // Extension methods za mapiranje između modela i DTO-ova
    public static class MappingExtensions
    {
        // Mapiranje Zaposleni -> ZaposleniDto
        public static ZaposleniDto ToDto(this Zaposleni zaposleni)
        {
            return new ZaposleniDto
            {
                Id = zaposleni.Id,
                Ime = zaposleni.Ime,
                Prezime = zaposleni.Prezime,
                Email = zaposleni.Email,
                Pozicija = zaposleni.PozicijaDisplay, // 🔧 FIXED: koristi computed property
                DatumZaposlenja = zaposleni.DatumZaposlenja,
                DatumRodjenja = zaposleni.DatumRodjenja,
                ImeOca = zaposleni.ImeOca,
                JMBG = zaposleni.JMBG,
                Adresa = zaposleni.Adresa,
                BrojTelefon = zaposleni.BrojTelefon,
                ProfileImageUrl = zaposleni.ProfileImageUrl,
                Pol = zaposleni.Pol,
                IsActive = zaposleni.IsActive,
                DatumKreiranja = zaposleni.DatumKreiranja,
                PunoIme = zaposleni.PunoIme,
                Godine = zaposleni.Godine,
                AvatarUrl = zaposleni.AvatarUrl,
                OdsekId = zaposleni.OdsekId,
                OdsekNaziv = zaposleni.Odsek?.Naziv
            };
        }

        // Mapiranje Odsek -> OdsekDto
        public static OdsekDto ToDto(this Odsek odsek)
        {
            return new OdsekDto
            {
                Id = odsek.Id,
                Naziv = odsek.Naziv,
                Opis = odsek.Opis,
                DatumKreiranja = odsek.DatumKreiranja,
                IsActive = odsek.IsActive,
                BrojZaposlenih = odsek.Zaposleni?.Count(z => z.IsActive) ?? 0
            };
        }

        // Mapiranje CreateZaposleniDto -> Zaposleni
        public static Zaposleni ToEntity(this CreateZaposleniDto dto)
        {
            return new Zaposleni
            {
                Ime = dto.Ime,
                Prezime = dto.Prezime,
                Email = dto.Email,
                PozicijaNaziv = dto.Pozicija, // 🔧 FIXED: koristi legacy polje
                DatumZaposlenja = dto.DatumZaposlenja,
                DatumRodjenja = dto.DatumRodjenja,
                ImeOca = dto.ImeOca,
                JMBG = dto.JMBG,
                Adresa = dto.Adresa,
                BrojTelefon = dto.BrojTelefon,
                Pol = dto.Pol,
                OdsekId = dto.OdsekId,
                IsActive = true,
                DatumKreiranja = DateTime.UtcNow
            };
        }

        // Update postojećeg Zaposleni entity-ja iz DTO-a
        public static void UpdateFromDto(this Zaposleni entity, CreateZaposleniDto dto)
        {
            entity.Ime = dto.Ime;
            entity.Prezime = dto.Prezime;
            entity.Email = dto.Email;
            entity.PozicijaNaziv = dto.Pozicija; // 🔧 FIXED: koristi legacy polje
            entity.DatumZaposlenja = dto.DatumZaposlenja;
            entity.DatumRodjenja = dto.DatumRodjenja;
            entity.ImeOca = dto.ImeOca;
            entity.JMBG = dto.JMBG;
            entity.Adresa = dto.Adresa;
            entity.BrojTelefon = dto.BrojTelefon;
            entity.Pol = dto.Pol;
            entity.OdsekId = dto.OdsekId;
        }
    }
}