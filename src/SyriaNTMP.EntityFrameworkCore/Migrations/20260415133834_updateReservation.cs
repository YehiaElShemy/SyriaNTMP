using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SyriaNTMP.Migrations
{
    /// <inheritdoc />
    public partial class updateReservation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Reservations",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "NumberOfRooms",
                table: "Reservations",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "City",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "NumberOfRooms",
                table: "Reservations");
        }
    }
}
