using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Hogwarts.Migrations
{
    /// <inheritdoc />
    public partial class AddInventarModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InventarStavke",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Naziv = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Kategorija = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SeriskiBroj = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Vrednost = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    DatumNabavke = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Lokacija = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Stanje = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Opis = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    QRKod = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Slika = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ZaposleniId = table.Column<int>(type: "integer", nullable: true),
                    DatumDodeljivanja = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DatumVracanja = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NapomenaKoriscenja = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DatumKreiranja = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DatumIzmene = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    KreiraOd = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IzmenioKorisnik = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventarStavke", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventarStavke_Zaposleni_ZaposleniId",
                        column: x => x.ZaposleniId,
                        principalTable: "Zaposleni",
                        principalColumn: "Id");
                });

            migrationBuilder.UpdateData(
                table: "Korisnici",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$j.QCYTe8uUmMeQcLyP8GW.cl1dRiDxfF0T/SecdQdZkIoWUJPbLkS");

            migrationBuilder.UpdateData(
                table: "Odseci",
                keyColumn: "Id",
                keyValue: 1,
                column: "DatumKreiranja",
                value: new DateTime(2025, 10, 10, 20, 19, 33, 230, DateTimeKind.Utc).AddTicks(6917));

            migrationBuilder.UpdateData(
                table: "Odseci",
                keyColumn: "Id",
                keyValue: 2,
                column: "DatumKreiranja",
                value: new DateTime(2025, 10, 10, 20, 19, 33, 230, DateTimeKind.Utc).AddTicks(6919));

            migrationBuilder.UpdateData(
                table: "Odseci",
                keyColumn: "Id",
                keyValue: 3,
                column: "DatumKreiranja",
                value: new DateTime(2025, 10, 10, 20, 19, 33, 230, DateTimeKind.Utc).AddTicks(6920));

            migrationBuilder.UpdateData(
                table: "Odseci",
                keyColumn: "Id",
                keyValue: 4,
                column: "DatumKreiranja",
                value: new DateTime(2025, 10, 10, 20, 19, 33, 230, DateTimeKind.Utc).AddTicks(6921));

            migrationBuilder.UpdateData(
                table: "Pozicije",
                keyColumn: "Id",
                keyValue: 1,
                column: "DatumKreiranja",
                value: new DateTime(2025, 10, 10, 20, 19, 33, 230, DateTimeKind.Utc).AddTicks(7052));

            migrationBuilder.UpdateData(
                table: "Pozicije",
                keyColumn: "Id",
                keyValue: 2,
                column: "DatumKreiranja",
                value: new DateTime(2025, 10, 10, 20, 19, 33, 230, DateTimeKind.Utc).AddTicks(7054));

            migrationBuilder.UpdateData(
                table: "Pozicije",
                keyColumn: "Id",
                keyValue: 3,
                column: "DatumKreiranja",
                value: new DateTime(2025, 10, 10, 20, 19, 33, 230, DateTimeKind.Utc).AddTicks(7055));

            migrationBuilder.UpdateData(
                table: "Pozicije",
                keyColumn: "Id",
                keyValue: 4,
                column: "DatumKreiranja",
                value: new DateTime(2025, 10, 10, 20, 19, 33, 230, DateTimeKind.Utc).AddTicks(7056));

            migrationBuilder.UpdateData(
                table: "Pozicije",
                keyColumn: "Id",
                keyValue: 5,
                column: "DatumKreiranja",
                value: new DateTime(2025, 10, 10, 20, 19, 33, 230, DateTimeKind.Utc).AddTicks(7057));

            migrationBuilder.UpdateData(
                table: "Pozicije",
                keyColumn: "Id",
                keyValue: 6,
                column: "DatumKreiranja",
                value: new DateTime(2025, 10, 10, 20, 19, 33, 230, DateTimeKind.Utc).AddTicks(7058));

            migrationBuilder.UpdateData(
                table: "Pozicije",
                keyColumn: "Id",
                keyValue: 7,
                column: "DatumKreiranja",
                value: new DateTime(2025, 10, 10, 20, 19, 33, 230, DateTimeKind.Utc).AddTicks(7102));

            migrationBuilder.UpdateData(
                table: "Pozicije",
                keyColumn: "Id",
                keyValue: 8,
                column: "DatumKreiranja",
                value: new DateTime(2025, 10, 10, 20, 19, 33, 230, DateTimeKind.Utc).AddTicks(7104));

            migrationBuilder.UpdateData(
                table: "Pozicije",
                keyColumn: "Id",
                keyValue: 9,
                column: "DatumKreiranja",
                value: new DateTime(2025, 10, 10, 20, 19, 33, 230, DateTimeKind.Utc).AddTicks(7105));

            migrationBuilder.UpdateData(
                table: "Pozicije",
                keyColumn: "Id",
                keyValue: 10,
                column: "DatumKreiranja",
                value: new DateTime(2025, 10, 10, 20, 19, 33, 230, DateTimeKind.Utc).AddTicks(7106));

            migrationBuilder.CreateIndex(
                name: "IX_InventarStavke_ZaposleniId",
                table: "InventarStavke",
                column: "ZaposleniId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InventarStavke");

            migrationBuilder.UpdateData(
                table: "Korisnici",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$RTgL0/d0vFuZXENTwdwcCOJAw2SAswIjxr6HfvnrFeGo8jAHtYPCC");

            migrationBuilder.UpdateData(
                table: "Odseci",
                keyColumn: "Id",
                keyValue: 1,
                column: "DatumKreiranja",
                value: new DateTime(2025, 10, 8, 17, 49, 1, 997, DateTimeKind.Utc).AddTicks(7273));

            migrationBuilder.UpdateData(
                table: "Odseci",
                keyColumn: "Id",
                keyValue: 2,
                column: "DatumKreiranja",
                value: new DateTime(2025, 10, 8, 17, 49, 1, 997, DateTimeKind.Utc).AddTicks(7275));

            migrationBuilder.UpdateData(
                table: "Odseci",
                keyColumn: "Id",
                keyValue: 3,
                column: "DatumKreiranja",
                value: new DateTime(2025, 10, 8, 17, 49, 1, 997, DateTimeKind.Utc).AddTicks(7276));

            migrationBuilder.UpdateData(
                table: "Odseci",
                keyColumn: "Id",
                keyValue: 4,
                column: "DatumKreiranja",
                value: new DateTime(2025, 10, 8, 17, 49, 1, 997, DateTimeKind.Utc).AddTicks(7277));

            migrationBuilder.UpdateData(
                table: "Pozicije",
                keyColumn: "Id",
                keyValue: 1,
                column: "DatumKreiranja",
                value: new DateTime(2025, 10, 8, 17, 49, 1, 997, DateTimeKind.Utc).AddTicks(7486));

            migrationBuilder.UpdateData(
                table: "Pozicije",
                keyColumn: "Id",
                keyValue: 2,
                column: "DatumKreiranja",
                value: new DateTime(2025, 10, 8, 17, 49, 1, 997, DateTimeKind.Utc).AddTicks(7488));

            migrationBuilder.UpdateData(
                table: "Pozicije",
                keyColumn: "Id",
                keyValue: 3,
                column: "DatumKreiranja",
                value: new DateTime(2025, 10, 8, 17, 49, 1, 997, DateTimeKind.Utc).AddTicks(7489));

            migrationBuilder.UpdateData(
                table: "Pozicije",
                keyColumn: "Id",
                keyValue: 4,
                column: "DatumKreiranja",
                value: new DateTime(2025, 10, 8, 17, 49, 1, 997, DateTimeKind.Utc).AddTicks(7490));

            migrationBuilder.UpdateData(
                table: "Pozicije",
                keyColumn: "Id",
                keyValue: 5,
                column: "DatumKreiranja",
                value: new DateTime(2025, 10, 8, 17, 49, 1, 997, DateTimeKind.Utc).AddTicks(7491));

            migrationBuilder.UpdateData(
                table: "Pozicije",
                keyColumn: "Id",
                keyValue: 6,
                column: "DatumKreiranja",
                value: new DateTime(2025, 10, 8, 17, 49, 1, 997, DateTimeKind.Utc).AddTicks(7492));

            migrationBuilder.UpdateData(
                table: "Pozicije",
                keyColumn: "Id",
                keyValue: 7,
                column: "DatumKreiranja",
                value: new DateTime(2025, 10, 8, 17, 49, 1, 997, DateTimeKind.Utc).AddTicks(7493));

            migrationBuilder.UpdateData(
                table: "Pozicije",
                keyColumn: "Id",
                keyValue: 8,
                column: "DatumKreiranja",
                value: new DateTime(2025, 10, 8, 17, 49, 1, 997, DateTimeKind.Utc).AddTicks(7494));

            migrationBuilder.UpdateData(
                table: "Pozicije",
                keyColumn: "Id",
                keyValue: 9,
                column: "DatumKreiranja",
                value: new DateTime(2025, 10, 8, 17, 49, 1, 997, DateTimeKind.Utc).AddTicks(7495));

            migrationBuilder.UpdateData(
                table: "Pozicije",
                keyColumn: "Id",
                keyValue: 10,
                column: "DatumKreiranja",
                value: new DateTime(2025, 10, 8, 17, 49, 1, 997, DateTimeKind.Utc).AddTicks(7497));
        }
    }
}
