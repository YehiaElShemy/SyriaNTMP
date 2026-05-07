using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SyriaNTMP.Migrations
{
    /// <inheritdoc />
    public partial class addCancelledDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledDate",
                table: "Reservations",
                type: "timestamp without time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancelledDate",
                table: "Reservations");
        }
    }
}
