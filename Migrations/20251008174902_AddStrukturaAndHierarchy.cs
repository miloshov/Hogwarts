using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Hogwarts.Migrations
{
    /// <inheritdoc />
    public partial class AddStrukturaAndHierarchy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Pozicija",
                table: "Zaposleni",
                newName: "PozicijaNaziv");

            migrationBuilder.AddColumn<int>(
                name: "NadredjeniId",
                table: "Zaposleni",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PozicijaId",
                table: "Zaposleni",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Pozicije",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Naziv = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Opis = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Nivo = table.Column<int>(type: "integer", maxLength: 50, nullable: false),
                    Boja = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    DatumKreiranja = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pozicije", x => x.Id);
                });

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

            migrationBuilder.InsertData(
                table: "Pozicije",
                columns: new[] { "Id", "Boja", "DatumKreiranja", "IsActive", "Naziv", "Nivo", "Opis" },
                values: new object[,]
                {
                    { 1, "#e74c3c", new DateTime(2025, 10, 8, 17, 49, 1, 997, DateTimeKind.Utc).AddTicks(7486), true, "CEO", 1, "Chief Executive Officer" },
                    { 2, "#e67e22", new DateTime(2025, 10, 8, 17, 49, 1, 997, DateTimeKind.Utc).AddTicks(7488), true, "CTO", 1, "Chief Technology Officer" },
                    { 3, "#f39c12", new DateTime(2025, 10, 8, 17, 49, 1, 997, DateTimeKind.Utc).AddTicks(7489), true, "CFO", 1, "Chief Financial Officer" },
                    { 4, "#27ae60", new DateTime(2025, 10, 8, 17, 49, 1, 997, DateTimeKind.Utc).AddTicks(7490), true, "HR Manager", 2, "Human Resources Manager" },
                    { 5, "#2980b9", new DateTime(2025, 10, 8, 17, 49, 1, 997, DateTimeKind.Utc).AddTicks(7491), true, "IT Manager", 2, "Information Technology Manager" },
                    { 6, "#8e44ad", new DateTime(2025, 10, 8, 17, 49, 1, 997, DateTimeKind.Utc).AddTicks(7492), true, "Senior Developer", 3, "Senior Software Developer" },
                    { 7, "#34495e", new DateTime(2025, 10, 8, 17, 49, 1, 997, DateTimeKind.Utc).AddTicks(7493), true, "Junior Developer", 4, "Junior Software Developer" },
                    { 8, "#16a085", new DateTime(2025, 10, 8, 17, 49, 1, 997, DateTimeKind.Utc).AddTicks(7494), true, "Business Analyst", 3, "Business Analyst" },
                    { 9, "#d35400", new DateTime(2025, 10, 8, 17, 49, 1, 997, DateTimeKind.Utc).AddTicks(7495), true, "QA Engineer", 3, "Quality Assurance Engineer" },
                    { 10, "#c0392b", new DateTime(2025, 10, 8, 17, 49, 1, 997, DateTimeKind.Utc).AddTicks(7497), true, "Marketing Specialist", 3, "Marketing Specialist" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Zaposleni_NadredjeniId",
                table: "Zaposleni",
                column: "NadredjeniId");

            migrationBuilder.CreateIndex(
                name: "IX_Zaposleni_PozicijaId",
                table: "Zaposleni",
                column: "PozicijaId");

            migrationBuilder.CreateIndex(
                name: "IX_Pozicije_Naziv",
                table: "Pozicije",
                column: "Naziv",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Zaposleni_Pozicije_PozicijaId",
                table: "Zaposleni",
                column: "PozicijaId",
                principalTable: "Pozicije",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Zaposleni_Zaposleni_NadredjeniId",
                table: "Zaposleni",
                column: "NadredjeniId",
                principalTable: "Zaposleni",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Zaposleni_Pozicije_PozicijaId",
                table: "Zaposleni");

            migrationBuilder.DropForeignKey(
                name: "FK_Zaposleni_Zaposleni_NadredjeniId",
                table: "Zaposleni");

            migrationBuilder.DropTable(
                name: "Pozicije");

            migrationBuilder.DropIndex(
                name: "IX_Zaposleni_NadredjeniId",
                table: "Zaposleni");

            migrationBuilder.DropIndex(
                name: "IX_Zaposleni_PozicijaId",
                table: "Zaposleni");

            migrationBuilder.DropColumn(
                name: "NadredjeniId",
                table: "Zaposleni");

            migrationBuilder.DropColumn(
                name: "PozicijaId",
                table: "Zaposleni");

            migrationBuilder.RenameColumn(
                name: "PozicijaNaziv",
                table: "Zaposleni",
                newName: "Pozicija");

            migrationBuilder.UpdateData(
                table: "Korisnici",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$eb09yJS9soHi2oT0bNjAh.n8mMF87QgLOzXDeZaa5suU4UNdPTu/q");

            migrationBuilder.UpdateData(
                table: "Odseci",
                keyColumn: "Id",
                keyValue: 1,
                column: "DatumKreiranja",
                value: new DateTime(2025, 10, 6, 20, 5, 17, 987, DateTimeKind.Utc).AddTicks(1300));

            migrationBuilder.UpdateData(
                table: "Odseci",
                keyColumn: "Id",
                keyValue: 2,
                column: "DatumKreiranja",
                value: new DateTime(2025, 10, 6, 20, 5, 17, 987, DateTimeKind.Utc).AddTicks(1303));

            migrationBuilder.UpdateData(
                table: "Odseci",
                keyColumn: "Id",
                keyValue: 3,
                column: "DatumKreiranja",
                value: new DateTime(2025, 10, 6, 20, 5, 17, 987, DateTimeKind.Utc).AddTicks(1304));

            migrationBuilder.UpdateData(
                table: "Odseci",
                keyColumn: "Id",
                keyValue: 4,
                column: "DatumKreiranja",
                value: new DateTime(2025, 10, 6, 20, 5, 17, 987, DateTimeKind.Utc).AddTicks(1305));
        }
    }
}
