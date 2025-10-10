using System.ComponentModel.DataAnnotations;

namespace Hogwarts.Models.DTOs
{
    // DTO za kreiranje nove stavke
    public class CreateInventarDto
    {
        [Required]
        [StringLength(200)]
        public string Naziv { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Kategorija { get; set; } = string.Empty;

        [StringLength(100)]
        public string? SeriskiBroj { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Vrednost mora biti veća od 0")]
        public decimal Vrednost { get; set; }

        [Required]
        public DateTime DatumNabavke { get; set; }

        [Required]
        [StringLength(200)]
        public string Lokacija { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Stanje { get; set; } = "Novo";

        [StringLength(1000)]
        public string? Opis { get; set; }

        [StringLength(200)]
        public string? Slika { get; set; }

        public int? ZaposleniId { get; set; }

        [StringLength(500)]
        public string? NapomenaKoriscenja { get; set; }
    }

    // DTO za ažuriranje stavke
    public class UpdateInventarDto
    {
        [Required]
        [StringLength(200)]
        public string Naziv { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Kategorija { get; set; } = string.Empty;

        [StringLength(100)]
        public string? SeriskiBroj { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Vrednost mora biti veća od 0")]
        public decimal Vrednost { get; set; }

        [Required]
        public DateTime DatumNabavke { get; set; }

        [Required]
        [StringLength(200)]
        public string Lokacija { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Stanje { get; set; }

        [StringLength(1000)]
        public string? Opis { get; set; }

        [StringLength(200)]
        public string? Slika { get; set; }

        [StringLength(500)]
        public string? NapomenaKoriscenja { get; set; }
    }

    // DTO za dodeljivanje/vraćanje
    public class DodelijInventarDto
    {
        [Required]
        public int ZaposleniId { get; set; }

        [StringLength(500)]
        public string? NapomenaKoriscenja { get; set; }
    }

    public class VratiInventarDto
    {
        [StringLength(500)]
        public string? NapomenaVracanja { get; set; }

        [StringLength(50)]
        public string? NovoStanje { get; set; }
    }

    // DTO za pretragu i filtriranje
    public class InventarFilterDto
    {
        public string? Pretraga { get; set; }
        public string? Kategorija { get; set; }
        public string? Stanje { get; set; }
        public string? Lokacija { get; set; }
        public int? ZaposleniId { get; set; }
        public bool? SamoDodeljene { get; set; }
        public bool? SamoDostupne { get; set; }
        public decimal? MinVrednost { get; set; }
        public decimal? MaxVrednost { get; set; }
        public DateTime? DatumNabavkeOd { get; set; }
        public DateTime? DatumNabavkeDo { get; set; }
        
        // Paginacija
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        
        // Sortiranje
        public string? SortBy { get; set; } = "DatumKreiranja";
        public string? SortDirection { get; set; } = "desc";
    }

    // DTO za response
    public class InventarResponseDto
    {
        public int Id { get; set; }
        public string Naziv { get; set; } = string.Empty;
        public string Kategorija { get; set; } = string.Empty;
        public string? SeriskiBroj { get; set; }
        public decimal Vrednost { get; set; }
        public DateTime DatumNabavke { get; set; }
        public string Lokacija { get; set; } = string.Empty;
        public string Stanje { get; set; } = string.Empty;
        public string? Opis { get; set; }
        public string? QRKod { get; set; }
        public string? Slika { get; set; }
        
        // Informacije o dodeli
        public int? ZaposleniId { get; set; }
        public string? ZaposleniImePrezime { get; set; }
        public DateTime? DatumDodeljivanja { get; set; }
        public DateTime? DatumVracanja { get; set; }
        public string? NapomenaKoriscenja { get; set; }
        
        // Computed fields
        public bool JeDodeljena { get; set; }
        public string StatusDisplay { get; set; } = string.Empty;
        public int DanaUKoriscenju { get; set; }
        
        // Audit
        public DateTime DatumKreiranja { get; set; }
        public DateTime? DatumIzmene { get; set; }
        public string? KreiraOd { get; set; }
        public string? IzmenioKorisnik { get; set; }
    }

    // DTO za statistike
    public class InventarStatistikeDto
    {
        public int UkupanBrojStavki { get; set; }
        public decimal UkupnaVrednost { get; set; }
        public int BrojDodeljenih { get; set; }
        public int BrojDostupnih { get; set; }
        public int BrojPokvarenih { get; set; }
        public int BrojIzgubljenih { get; set; }
        
        public List<KategorijaStatistika> StatistikePoKategorijama { get; set; } = new();
        public List<StanjeStatistika> StatistikePoStanjima { get; set; } = new();
        public List<LokacijaStatistika> StatistikePoLokacijama { get; set; } = new();
    }

    public class KategorijaStatistika
    {
        public string Kategorija { get; set; } = string.Empty;
        public int Broj { get; set; }
        public decimal UkupnaVrednost { get; set; }
        public decimal ProcentOdUkupne { get; set; }
    }

    public class StanjeStatistika
    {
        public string Stanje { get; set; } = string.Empty;
        public int Broj { get; set; }
        public decimal ProcentOdUkupne { get; set; }
    }

    public class LokacijaStatistika
    {
        public string Lokacija { get; set; } = string.Empty;
        public int Broj { get; set; }
        public decimal UkupnaVrednost { get; set; }
    }

    // DTO za paginirane rezultate
    public class InventarPagedResult
    {
        public List<InventarResponseDto> Data { get; set; } = new();
        public PaginationInfo Pagination { get; set; } = new();
    }

    public class PaginationInfo
    {
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public bool HasNext { get; set; }
        public bool HasPrevious { get; set; }
    }
}