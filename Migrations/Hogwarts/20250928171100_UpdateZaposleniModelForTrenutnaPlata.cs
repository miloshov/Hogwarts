using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hogwarts.Migrations.Hogwarts
{
    /// <inheritdoc />
    public partial class UpdateZaposleniModelForTrenutnaPlata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Odeljenje",
                table: "Zaposleni",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Odeljenje",
                table: "Zaposleni");
        }
    }
}
