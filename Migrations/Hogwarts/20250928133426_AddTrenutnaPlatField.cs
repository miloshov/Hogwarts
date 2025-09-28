using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hogwarts.Migrations.Hogwarts
{
    /// <inheritdoc />
    public partial class AddTrenutnaPlatField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "TrenutnaPlata",
                table: "Zaposleni",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TrenutnaPlata",
                table: "Zaposleni");
        }
    }
}
