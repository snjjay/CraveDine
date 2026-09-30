using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EatKath.API.Migrations
{
    /// <inheritdoc />
    public partial class AddRedemptionReservationId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReservationId",
                table: "Redemptions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Redemptions_ReservationId",
                table: "Redemptions",
                column: "ReservationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Redemptions_Reservations_ReservationId",
                table: "Redemptions",
                column: "ReservationId",
                principalTable: "Reservations",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Redemptions_Reservations_ReservationId",
                table: "Redemptions");

            migrationBuilder.DropIndex(
                name: "IX_Redemptions_ReservationId",
                table: "Redemptions");

            migrationBuilder.DropColumn(
                name: "ReservationId",
                table: "Redemptions");
        }
    }
}
