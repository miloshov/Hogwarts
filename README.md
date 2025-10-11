# Hogwarts HR - Backend

## 📋 Pregled

Backend aplikacija za Hogwarts HR sistem izgrađena u .NET 8 sa Entity Framework Core, PostgreSQL bazom podataka i JWT autentifikacijom.

## 🚀 Funkcionalnosti

### 👥 Upravljanje Zaposlenima
- CRUD operacije za zaposlene
- Pretraga i filtriranje
- Audit trail funkcionalnost

### 💰 Plate
- Upravljanje platama zaposlenih
- Mesečni obračuni
- Izveštavanje

### 🏖️ Zahtevi za Odmor
- Kreiranje i upravljanje zahtevima
- Workflow odobravanje
- Kalendar odmora

### 📦 **NOVO: Inventar Modul**
- Upravljanje inventarskim stavkama
- QR kod generisanje
- Dodela/vraćanje opreme korisnicima
- Statistike i izveštavanje
- Kategorije i lokacije

## 🛠️ Tehnologije

- **.NET 8** - Core framework
- **Entity Framework Core** - ORM
- **PostgreSQL** - Baza podataka
- **JWT** - Autentifikacija
- **QRCoder** - QR kod generisanje
- **AutoMapper** - Mapiranje objekata

## 📚 API Dokumentacija

### Inventar Endpoints

| Method | Endpoint | Opis |
|--------|----------|------|
| GET | `/api/inventar` | Lista svih inventarskih stavki |
| GET | `/api/inventar/{id}` | Detalji specifične stavke |
| POST | `/api/inventar` | Kreiranje nove stavke |
| PUT | `/api/inventar/{id}` | Ažuriranje stavke |
| DELETE | `/api/inventar/{id}` | Brisanje stavke |
| POST | `/api/inventar/{id}/dodeli` | Dodela stavke korisniku |
| POST | `/api/inventar/{id}/vrati` | Vraćanje stavke |
| GET | `/api/inventar/statistike` | Statistike inventara |
| GET | `/api/inventar/{id}/qr` | QR kod za stavku |

## 🗃️ Baza Podataka

### InventarStavka Tabela
```sql
- Id (int, PK)
- Naziv (string, required)
- Opis (string, optional)
- KategorijaId (int)
- LokacijaId (int)
- SerijskiBroj (string, optional)
- BarKod (string, optional)
- Stanje (string: Novo, Dobro, Zadovoljavajuce, Loshe)
- NabavnaCena (decimal, optional)
- TrenutnaVrednost (decimal, optional)
- DatumNabavke (DateTime, optional)
- GarancijaDo (DateTime, optional)
- DodeljenaKorisnikuId (int, optional)
- DatumKreiranja (DateTime)
- DatumIzmene (DateTime, optional)
- IsActive (bool)
```

## 🔄 Migracije

Poslednja migracija: `AddInventarModule`

```bash
# Primeni migracije
dotnet ef database update --context HogwartsContext

# Kreiraj novu migraciju
dotnet ef migrations add [MigrationName] --context HogwartsContext
```

## ⚙️ Pokretanje

1. Kloniraj repozitorijum
2. Konfiguriši connection string u `appsettings.json`
3. Pokreni migracije:
   ```bash
   dotnet ef database update --context HogwartsContext
   ```
4. Pokreni aplikaciju:
   ```bash
   dotnet run
   ```

## 📝 Konfiguracija

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=HogwartsHR;Username=your_user;Password=your_password"
  },
  "Jwt": {
    "Key": "your_secret_key",
    "Issuer": "HogwartsHR",
    "Audience": "HogwartsHR"
  }
}
```

## 🔐 Autentifikacija

API koristi JWT tokene. Uključi token u Authorization header:
```
Authorization: Bearer <your_jwt_token>
```

## 🧪 Status

✅ **Kompletno:** Zaposleni, Plate, Zahtevi za Odmor  
🚧 **U razvoju:** Inventar modul (backend završen, frontend u toku)  
📋 **Planirano:** Izveštavanje, Notifikacije  

---

**Autor:** MiniMax Agent  
<<<<<<< HEAD
**Datum poslednje izmene:** {{ current_date }}
=======
**Datum poslednje izmene:** {{ current_date }}
>>>>>>> ed2d72a533b9e685713351ec79fca7830e7b14c6
