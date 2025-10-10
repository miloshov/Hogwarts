using Microsoft.EntityFrameworkCore;
using Hogwarts.Data;
using Hogwarts.Models;
using Hogwarts.Models.DTOs;
using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;

namespace Hogwarts.Services
{
    public interface IInventarService
    {
        Task<InventarPagedResult> GetInventarPaginatedAsync(InventarFilterDto filter);
        Task<InventarResponseDto?> GetInventarByIdAsync(int id);
        Task<InventarResponseDto> CreateInventarAsync(CreateInventarDto dto, string korisnik);
        Task<InventarResponseDto> UpdateInventarAsync(int id, UpdateInventarDto dto, string korisnik);
        Task<bool> DeleteInventarAsync(int id);
        Task<bool> DodelijInventarAsync(int id, DodelijInventarDto dto, string korisnik);
        Task<bool> VratiInventarAsync(int id, VratiInventarDto dto, string korisnik);
        Task<InventarStatistikeDto> GetStatistikeAsync();
        Task<byte[]> GenerateQRCodeAsync(int id);
        Task<List<string>> GetKategorije();
        Task<List<string>> GetLokacije();
        Task<List<InventarResponseDto>> GetInventarByZaposleniAsync(int zaposleniId);
    }

    public class InventarService : IInventarService
    {
        private readonly HogwartsContext _context;

        public InventarService(HogwartsContext context)
        {
            _context = context;
        }

        public async Task<InventarPagedResult> GetInventarPaginatedAsync(InventarFilterDto filter)
        {
            var query = _context.InventarStavke
                .Include(i => i.Zaposleni)
                .Where(i => i.IsActive);

            // Filtriranje
            if (!string.IsNullOrWhiteSpace(filter.Pretraga))
            {
                var search = filter.Pretraga.ToLower();
                query = query.Where(i => 
                    i.Naziv.ToLower().Contains(search) ||
                    i.SeriskiBroj.ToLower().Contains(search) ||
                    i.Opis.ToLower().Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(filter.Kategorija))
            {
                query = query.Where(i => i.Kategorija == filter.Kategorija);
            }

            if (!string.IsNullOrWhiteSpace(filter.Stanje))
            {
                query = query.Where(i => i.Stanje == filter.Stanje);
            }

            if (!string.IsNullOrWhiteSpace(filter.Lokacija))
            {
                query = query.Where(i => i.Lokacija == filter.Lokacija);
            }

            if (filter.ZaposleniId.HasValue)
            {
                query = query.Where(i => i.ZaposleniId == filter.ZaposleniId.Value);
            }

            if (filter.SamoDodeljene.HasValue && filter.SamoDodeljene.Value)
            {
                query = query.Where(i => i.ZaposleniId.HasValue && !i.DatumVracanja.HasValue);
            }

            if (filter.SamoDostupne.HasValue && filter.SamoDostupne.Value)
            {
                query = query.Where(i => !i.ZaposleniId.HasValue || i.DatumVracanja.HasValue);
            }

            if (filter.MinVrednost.HasValue)
            {
                query = query.Where(i => i.Vrednost >= filter.MinVrednost.Value);
            }

            if (filter.MaxVrednost.HasValue)
            {
                query = query.Where(i => i.Vrednost <= filter.MaxVrednost.Value);
            }

            if (filter.DatumNabavkeOd.HasValue)
            {
                query = query.Where(i => i.DatumNabavke >= filter.DatumNabavkeOd.Value);
            }

            if (filter.DatumNabavkeDo.HasValue)
            {
                query = query.Where(i => i.DatumNabavke <= filter.DatumNabavkeDo.Value);
            }

            // Sortiranje
            query = filter.SortBy?.ToLower() switch
            {
                "naziv" => filter.SortDirection == "asc" ? 
                    query.OrderBy(i => i.Naziv) : query.OrderByDescending(i => i.Naziv),
                "kategorija" => filter.SortDirection == "asc" ? 
                    query.OrderBy(i => i.Kategorija) : query.OrderByDescending(i => i.Kategorija),
                "vrednost" => filter.SortDirection == "asc" ? 
                    query.OrderBy(i => i.Vrednost) : query.OrderByDescending(i => i.Vrednost),
                "datumnabavke" => filter.SortDirection == "asc" ? 
                    query.OrderBy(i => i.DatumNabavke) : query.OrderByDescending(i => i.DatumNabavke),
                "stanje" => filter.SortDirection == "asc" ? 
                    query.OrderBy(i => i.Stanje) : query.OrderByDescending(i => i.Stanje),
                _ => query.OrderByDescending(i => i.DatumKreiranja)
            };

            // Ukupan broj
            var totalCount = await query.CountAsync();

            // Paginacija
            var items = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(i => new InventarResponseDto
                {
                    Id = i.Id,
                    Naziv = i.Naziv,
                    Kategorija = i.Kategorija,
                    SeriskiBroj = i.SeriskiBroj,
                    Vrednost = i.Vrednost,
                    DatumNabavke = i.DatumNabavke,
                    Lokacija = i.Lokacija,
                    Stanje = i.Stanje,
                    Opis = i.Opis,
                    QRKod = i.QRKod,
                    Slika = i.Slika,
                    ZaposleniId = i.ZaposleniId,
                    ZaposleniImePrezime = i.Zaposleni != null ? i.Zaposleni.PunoIme : null,
                    DatumDodeljivanja = i.DatumDodeljivanja,
                    DatumVracanja = i.DatumVracanja,
                    NapomenaKoriscenja = i.NapomenaKoriscenja,
                    JeDodeljena = i.ZaposleniId.HasValue && !i.DatumVracanja.HasValue,
                    StatusDisplay = i.ZaposleniId.HasValue && !i.DatumVracanja.HasValue ? 
                        $"Dodeljena - {i.Zaposleni.PunoIme}" : "Dostupna",
                    DanaUKoriscenju = i.ZaposleniId.HasValue && !i.DatumVracanja.HasValue && i.DatumDodeljivanja.HasValue ? 
                        (DateTime.UtcNow - i.DatumDodeljivanja.Value).Days : 0,
                    DatumKreiranja = i.DatumKreiranja,
                    DatumIzmene = i.DatumIzmene,
                    KreiraOd = i.KreiraOd,
                    IzmenioKorisnik = i.IzmenioKorisnik
                })
                .ToListAsync();

            var totalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize);

            return new InventarPagedResult
            {
                Data = items,
                Pagination = new PaginationInfo
                {
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                    CurrentPage = filter.Page,
                    PageSize = filter.PageSize,
                    HasNext = filter.Page < totalPages,
                    HasPrevious = filter.Page > 1
                }
            };
        }

        public async Task<InventarResponseDto?> GetInventarByIdAsync(int id)
        {
            var stavka = await _context.InventarStavke
                .Include(i => i.Zaposleni)
                .Where(i => i.Id == id && i.IsActive)
                .FirstOrDefaultAsync();

            if (stavka == null)
                return null;

            return new InventarResponseDto
            {
                Id = stavka.Id,
                Naziv = stavka.Naziv,
                Kategorija = stavka.Kategorija,
                SeriskiBroj = stavka.SeriskiBroj,
                Vrednost = stavka.Vrednost,
                DatumNabavke = stavka.DatumNabavke,
                Lokacija = stavka.Lokacija,
                Stanje = stavka.Stanje,
                Opis = stavka.Opis,
                QRKod = stavka.QRKod,
                Slika = stavka.Slika,
                ZaposleniId = stavka.ZaposleniId,
                ZaposleniImePrezime = stavka.Zaposleni?.PunoIme,
                DatumDodeljivanja = stavka.DatumDodeljivanja,
                DatumVracanja = stavka.DatumVracanja,
                NapomenaKoriscenja = stavka.NapomenaKoriscenja,
                JeDodeljena = stavka.JeDodeljena,
                StatusDisplay = stavka.StatusDisplay,
                DanaUKoriscenju = stavka.DanaUKoriscenju,
                DatumKreiranja = stavka.DatumKreiranja,
                DatumIzmene = stavka.DatumIzmene,
                KreiraOd = stavka.KreiraOd,
                IzmenioKorisnik = stavka.IzmenioKorisnik
            };
        }

        public async Task<InventarResponseDto> CreateInventarAsync(CreateInventarDto dto, string korisnik)
        {
            var stavka = new InventarStavka
            {
                Naziv = dto.Naziv,
                Kategorija = dto.Kategorija,
                SeriskiBroj = dto.SeriskiBroj,
                Vrednost = dto.Vrednost,
                DatumNabavke = dto.DatumNabavke,
                Lokacija = dto.Lokacija,
                Stanje = dto.Stanje,
                Opis = dto.Opis,
                Slika = dto.Slika,
                ZaposleniId = dto.ZaposleniId,
                NapomenaKoriscenja = dto.NapomenaKoriscenja,
                DatumDodeljivanja = dto.ZaposleniId.HasValue ? DateTime.UtcNow : null,
                DatumKreiranja = DateTime.UtcNow,
                KreiraOd = korisnik,
                IsActive = true
            };

            _context.InventarStavke.Add(stavka);
            await _context.SaveChangesAsync();

            // Generiši QR kod
            stavka.QRKod = GenerateQRCodeData(stavka.Id);
            await _context.SaveChangesAsync();

            return await GetInventarByIdAsync(stavka.Id) ?? throw new Exception("Greška pri kreiranju inventara");
        }

        public async Task<InventarResponseDto> UpdateInventarAsync(int id, UpdateInventarDto dto, string korisnik)
        {
            var stavka = await _context.InventarStavke.FindAsync(id);
            if (stavka == null || !stavka.IsActive)
                throw new ArgumentException("Inventar stavka nije pronađena");

            stavka.Naziv = dto.Naziv;
            stavka.Kategorija = dto.Kategorija;
            stavka.SeriskiBroj = dto.SeriskiBroj;
            stavka.Vrednost = dto.Vrednost;
            stavka.DatumNabavke = dto.DatumNabavke;
            stavka.Lokacija = dto.Lokacija;
            stavka.Stanje = dto.Stanje;
            stavka.Opis = dto.Opis;
            stavka.Slika = dto.Slika;
            stavka.NapomenaKoriscenja = dto.NapomenaKoriscenja;
            stavka.DatumIzmene = DateTime.UtcNow;
            stavka.IzmenioKorisnik = korisnik;

            await _context.SaveChangesAsync();

            return await GetInventarByIdAsync(id) ?? throw new Exception("Greška pri ažuriranju inventara");
        }

        public async Task<bool> DeleteInventarAsync(int id)
        {
            var stavka = await _context.InventarStavke.FindAsync(id);
            if (stavka == null || !stavka.IsActive)
                return false;

            stavka.IsActive = false;
            stavka.DatumIzmene = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DodelijInventarAsync(int id, DodelijInventarDto dto, string korisnik)
        {
            var stavka = await _context.InventarStavke.FindAsync(id);
            if (stavka == null || !stavka.IsActive)
                return false;

            // Proveri da li je zaposleni validan
            var zaposleni = await _context.Zaposleni.FindAsync(dto.ZaposleniId);
            if (zaposleni == null || !zaposleni.IsActive)
                return false;

            stavka.ZaposleniId = dto.ZaposleniId;
            stavka.DatumDodeljivanja = DateTime.UtcNow;
            stavka.DatumVracanja = null;
            stavka.NapomenaKoriscenja = dto.NapomenaKoriscenja;
            stavka.DatumIzmene = DateTime.UtcNow;
            stavka.IzmenioKorisnik = korisnik;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> VratiInventarAsync(int id, VratiInventarDto dto, string korisnik)
        {
            var stavka = await _context.InventarStavke.FindAsync(id);
            if (stavka == null || !stavka.IsActive || !stavka.ZaposleniId.HasValue)
                return false;

            stavka.DatumVracanja = DateTime.UtcNow;
            stavka.ZaposleniId = null; // Oslobodi inventar
            
            if (!string.IsNullOrWhiteSpace(dto.NovoStanje))
            {
                stavka.Stanje = dto.NovoStanje;
            }

            if (!string.IsNullOrWhiteSpace(dto.NapomenaVracanja))
            {
                stavka.NapomenaKoriscenja += $"\n[Vraćeno {DateTime.UtcNow:dd.MM.yyyy}]: {dto.NapomenaVracanja}";
            }

            stavka.DatumIzmene = DateTime.UtcNow;
            stavka.IzmenioKorisnik = korisnik;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<InventarStatistikeDto> GetStatistikeAsync()
        {
            var stavke = await _context.InventarStavke
                .Where(i => i.IsActive)
                .ToListAsync();

            var ukupanBroj = stavke.Count;
            var ukupnaVrednost = stavke.Sum(i => i.Vrednost);

            var statistike = new InventarStatistikeDto
            {
                UkupanBrojStavki = ukupanBroj,
                UkupnaVrednost = ukupnaVrednost,
                BrojDodeljenih = stavke.Count(i => i.JeDodeljena),
                BrojDostupnih = stavke.Count(i => !i.JeDodeljena),
                BrojPokvarenih = stavke.Count(i => i.Stanje == InventarStanja.Pokvareno),
                BrojIzgubljenih = stavke.Count(i => i.Stanje == InventarStanja.Izgubljeno),

                StatistikePoKategorijama = stavke
                    .GroupBy(i => i.Kategorija)
                    .Select(g => new KategorijaStatistika
                    {
                        Kategorija = g.Key,
                        Broj = g.Count(),
                        UkupnaVrednost = g.Sum(i => i.Vrednost),
                        ProcentOdUkupne = ukupanBroj > 0 ? (decimal)g.Count() / ukupanBroj * 100 : 0
                    })
                    .OrderByDescending(k => k.UkupnaVrednost)
                    .ToList(),

                StatistikePoStanjima = stavke
                    .GroupBy(i => i.Stanje)
                    .Select(g => new StanjeStatistika
                    {
                        Stanje = g.Key,
                        Broj = g.Count(),
                        ProcentOdUkupne = ukupanBroj > 0 ? (decimal)g.Count() / ukupanBroj * 100 : 0
                    })
                    .OrderByDescending(s => s.Broj)
                    .ToList(),

                StatistikePoLokacijama = stavke
                    .GroupBy(i => i.Lokacija)
                    .Select(g => new LokacijaStatistika
                    {
                        Lokacija = g.Key,
                        Broj = g.Count(),
                        UkupnaVrednost = g.Sum(i => i.Vrednost)
                    })
                    .OrderByDescending(l => l.UkupnaVrednost)
                    .ToList()
            };

            return statistike;
        }

        public async Task<byte[]> GenerateQRCodeAsync(int id)
        {
            var qrGenerator = new QRCodeGenerator();
            var qrData = GenerateQRCodeData(id);
            var qrCodeData = qrGenerator.CreateQrCode(qrData, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new QRCode(qrCodeData);
            
            using var qrBitmap = qrCode.GetGraphic(20);
            using var stream = new MemoryStream();
            qrBitmap.Save(stream, ImageFormat.Png);
            return stream.ToArray();
        }

        public async Task<List<string>> GetKategorije()
        {
            var customKategorije = await _context.InventarStavke
                .Where(i => i.IsActive)
                .Select(i => i.Kategorija)
                .Distinct()
                .ToListAsync();

            var sveKategorije = InventarKategorije.SveKategorije
                .Union(customKategorije)
                .Distinct()
                .OrderBy(k => k)
                .ToList();

            return sveKategorije;
        }

        public async Task<List<string>> GetLokacije()
        {
            return await _context.InventarStavke
                .Where(i => i.IsActive)
                .Select(i => i.Lokacija)
                .Distinct()
                .OrderBy(l => l)
                .ToListAsync();
        }

        public async Task<List<InventarResponseDto>> GetInventarByZaposleniAsync(int zaposleniId)
        {
            return await _context.InventarStavke
                .Include(i => i.Zaposleni)
                .Where(i => i.IsActive && i.ZaposleniId == zaposleniId && !i.DatumVracanja.HasValue)
                .Select(i => new InventarResponseDto
                {
                    Id = i.Id,
                    Naziv = i.Naziv,
                    Kategorija = i.Kategorija,
                    SeriskiBroj = i.SeriskiBroj,
                    Vrednost = i.Vrednost,
                    DatumNabavke = i.DatumNabavke,
                    Lokacija = i.Lokacija,
                    Stanje = i.Stanje,
                    DatumDodeljivanja = i.DatumDodeljivanja,
                    NapomenaKoriscenja = i.NapomenaKoriscenja,
                    DanaUKoriscenju = i.DatumDodeljivanja.HasValue ? 
                        (DateTime.UtcNow - i.DatumDodeljivanja.Value).Days : 0
                })
                .ToListAsync();
        }

        private string GenerateQRCodeData(int id)
        {
            // Generiši QR kod koji sadrži link ka inventaru
            return $"https://hogwarts-hr.com/inventar/{id}";
        }
    }
}