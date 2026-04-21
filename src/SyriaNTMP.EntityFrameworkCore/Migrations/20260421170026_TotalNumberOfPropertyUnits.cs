using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SyriaNTMP.Migrations
{
    /// <inheritdoc />
    public partial class TotalNumberOfPropertyUnits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TotalNumberOfPropertyUnits",
                table: "Reservations",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalNumberOfPropertyUnits",
                table: "Reservations");
        }
    }
}
