using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EatKath.API.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueActiveReservationIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reservations_UserId",
                table: "Reservations");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_UserId_DealId_ReservationDate_ReservationTime",
                table: "Reservations",
                columns: new[] { "UserId", "DealId", "ReservationDate", "ReservationTime" },
                unique: true,
                filter: "[Status] <> N'Cancelled' AND [Status] <> N'Rejected' AND [Status] <> N'NoShow'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reservations_UserId_DealId_ReservationDate_ReservationTime",
                table: "Reservations");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_UserId",
                table: "Reservations",
                column: "UserId");
        }
    }
}
