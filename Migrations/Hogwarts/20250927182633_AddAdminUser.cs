using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hogwarts.Migrations.Hogwarts
{
    /// <inheritdoc />
    public partial class AddAdminUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Korisnici",
                columns: new[] { "Id", "DatumRegistracije", "Email", "IsActive", "PasswordHash", "PoslednjePrijavljivanje", "Role", "UserName", "ZaposleniId" },
                values: new object[] { 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "admin@hogwarts.com", true, "admin123", null, "Admin", "admin", null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Korisnici",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
