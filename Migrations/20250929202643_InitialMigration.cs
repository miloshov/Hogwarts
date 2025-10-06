using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Hogwarts.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Odseci",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Naziv = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Opis = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DatumKreiranja = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Odseci", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Zaposleni",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Ime = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Prezime = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Pozicija = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DatumZaposlenja = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DatumRodjenja = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ImeOca = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    JMBG = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
                    Adresa = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    BrojTelefon = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    DatumKreiranja = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OdsekId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Zaposleni", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Zaposleni_Odseci_OdsekId",
                        column: x => x.OdsekId,
                        principalTable: "Odseci",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Korisnici",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    DatumRegistracije = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PoslednjePrijavljivanje = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ZaposleniId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Korisnici", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Korisnici_Zaposleni_ZaposleniId",
                        column: x => x.ZaposleniId,
                        principalTable: "Zaposleni",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Plate",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ZaposleniId = table.Column<int>(type: "integer", nullable: false),
                    Osnovna = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    Bonusi = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    Otkazi = table.Column<decimal>(type: "numeric", nullable: false),
                    Period = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    DatumKreiranja = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Napomene = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plate", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Plate_Zaposleni_ZaposleniId",
                        column: x => x.ZaposleniId,
                        principalTable: "Zaposleni",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ZahteviZaOdmor",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ZaposleniId = table.Column<int>(type: "integer", nullable: false),
                    DatumOd = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DatumDo = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Razlog = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TipOdmora = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DatumZahteva = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DatumOdgovora = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    OdobrioKorisnikId = table.Column<int>(type: "integer", nullable: true),
                    NapomenaOdgovora = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZahteviZaOdmor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ZahteviZaOdmor_Zaposleni_ZaposleniId",
                        column: x => x.ZaposleniId,
                        principalTable: "Zaposleni",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Korisnici",
                columns: new[] { "Id", "DatumRegistracije", "Email", "IsActive", "PasswordHash", "PoslednjePrijavljivanje", "Role", "UserName", "ZaposleniId" },
                values: new object[] { 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "admin@hogwarts.rs", true, "$2a$11$lCqc0M/XcxftkQHngRfjXugQG9cFPQ/cLmMSfpzlUNnqY8gvkA0Eu", null, "SuperAdmin", "admin", null });

            migrationBuilder.InsertData(
                table: "Odseci",
                columns: new[] { "Id", "DatumKreiranja", "IsActive", "Naziv", "Opis" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 9, 29, 20, 26, 43, 274, DateTimeKind.Utc).AddTicks(3915), true, "IT", "Informacione tehnologije" },
                    { 2, new DateTime(2025, 9, 29, 20, 26, 43, 274, DateTimeKind.Utc).AddTicks(3918), true, "HR", "Ljudski resursi" },
                    { 3, new DateTime(2025, 9, 29, 20, 26, 43, 274, DateTimeKind.Utc).AddTicks(3919), true, "Finansije", "Finansijski sektor" },
                    { 4, new DateTime(2025, 9, 29, 20, 26, 43, 274, DateTimeKind.Utc).AddTicks(3920), true, "Marketing", "Marketing i prodaja" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Korisnici_Email",
                table: "Korisnici",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Korisnici_UserName",
                table: "Korisnici",
                column: "UserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Korisnici_ZaposleniId",
                table: "Korisnici",
                column: "ZaposleniId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Plate_ZaposleniId",
                table: "Plate",
                column: "ZaposleniId");

            migrationBuilder.CreateIndex(
                name: "IX_ZahteviZaOdmor_ZaposleniId",
                table: "ZahteviZaOdmor",
                column: "ZaposleniId");

            migrationBuilder.CreateIndex(
                name: "IX_Zaposleni_OdsekId",
                table: "Zaposleni",
                column: "OdsekId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Korisnici");

            migrationBuilder.DropTable(
                name: "Plate");

            migrationBuilder.DropTable(
                name: "ZahteviZaOdmor");

            migrationBuilder.DropTable(
                name: "Zaposleni");

            migrationBuilder.DropTable(
                name: "Odseci");
        }
    }
}
