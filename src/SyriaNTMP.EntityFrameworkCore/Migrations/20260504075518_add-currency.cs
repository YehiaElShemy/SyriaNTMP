using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SyriaNTMP.Migrations
{
    /// <inheritdoc />
    public partial class addcurrency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurrencyId",
                table: "Reservations",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CurrencySymbolAr",
                table: "Reservations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CurrencySymbolEn",
                table: "Reservations",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrencyId",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "CurrencySymbolAr",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "CurrencySymbolEn",
                table: "Reservations");
        }
    }
}
