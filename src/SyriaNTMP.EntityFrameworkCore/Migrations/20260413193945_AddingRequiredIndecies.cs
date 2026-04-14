using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SyriaNTMP.Migrations
{
    /// <inheritdoc />
    public partial class AddingRequiredIndecies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Reservations_CompanyId",
                table: "Reservations",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_CreatedDate",
                table: "Reservations",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_FromDate",
                table: "Reservations",
                column: "FromDate");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_PropertyId",
                table: "Reservations",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_PropertyRating",
                table: "Reservations",
                column: "PropertyRating");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_ReservationId",
                table: "Reservations",
                column: "ReservationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_ReservationPurpose",
                table: "Reservations",
                column: "ReservationPurpose");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_ReservationStatus",
                table: "Reservations",
                column: "ReservationStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_ToDate",
                table: "Reservations",
                column: "ToDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reservations_CompanyId",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_CreatedDate",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_FromDate",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_PropertyId",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_PropertyRating",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_ReservationId",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_ReservationPurpose",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_ReservationStatus",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_ToDate",
                table: "Reservations");
        }
    }
}
