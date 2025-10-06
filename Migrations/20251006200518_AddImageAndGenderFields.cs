using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hogwarts.Migrations
{
    /// <inheritdoc />
    public partial class AddImageAndGenderFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Pol",
                table: "Zaposleni",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ProfileImageUrl",
                table: "Zaposleni",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Pol",
                table: "Zaposleni");

            migrationBuilder.DropColumn(
                name: "ProfileImageUrl",
                table: "Zaposleni");

            migrationBuilder.UpdateData(
                table: "Korisnici",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$lCqc0M/XcxftkQHngRfjXugQG9cFPQ/cLmMSfpzlUNnqY8gvkA0Eu");

            migrationBuilder.UpdateData(
                table: "Odseci",
                keyColumn: "Id",
                keyValue: 1,
                column: "DatumKreiranja",
                value: new DateTime(2025, 9, 29, 20, 26, 43, 274, DateTimeKind.Utc).AddTicks(3915));

            migrationBuilder.UpdateData(
                table: "Odseci",
                keyColumn: "Id",
                keyValue: 2,
                column: "DatumKreiranja",
                value: new DateTime(2025, 9, 29, 20, 26, 43, 274, DateTimeKind.Utc).AddTicks(3918));

            migrationBuilder.UpdateData(
                table: "Odseci",
                keyColumn: "Id",
                keyValue: 3,
                column: "DatumKreiranja",
                value: new DateTime(2025, 9, 29, 20, 26, 43, 274, DateTimeKind.Utc).AddTicks(3919));

            migrationBuilder.UpdateData(
                table: "Odseci",
                keyColumn: "Id",
                keyValue: 4,
                column: "DatumKreiranja",
                value: new DateTime(2025, 9, 29, 20, 26, 43, 274, DateTimeKind.Utc).AddTicks(3920));
        }
    }
}
