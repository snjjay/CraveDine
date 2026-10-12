using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EatKath.API.Migrations
{
    /// <inheritdoc />
    public partial class AddSurpriseBags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SurpriseBags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RestaurantId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Category = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Conditions = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    EstimatedValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    QuantityTotal = table.Column<int>(type: "int", nullable: false),
                    MaxPerCustomer = table.Column<int>(type: "int", nullable: false),
                    PickupDate = table.Column<DateOnly>(type: "date", nullable: false),
                    PickupStartTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    PickupEndTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SurpriseBags", x => x.Id);
                    table.CheckConstraint("CK_SurpriseBags_EstimatedValue", "[EstimatedValue] IS NULL OR [EstimatedValue] >= 0");
                    table.CheckConstraint("CK_SurpriseBags_MaxPerCustomer", "[MaxPerCustomer] >= 1");
                    table.CheckConstraint("CK_SurpriseBags_PickupWindow", "[PickupEndTime] > [PickupStartTime]");
                    table.CheckConstraint("CK_SurpriseBags_Price", "[Price] >= 0");
                    table.CheckConstraint("CK_SurpriseBags_QuantityTotal", "[QuantityTotal] >= 1");
                    table.ForeignKey(
                        name: "FK_SurpriseBags_Restaurants_RestaurantId",
                        column: x => x.RestaurantId,
                        principalTable: "Restaurants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SurpriseBagReservations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SurpriseBagId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ConfirmationCode = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CollectedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SurpriseBagReservations", x => x.Id);
                    table.CheckConstraint("CK_SurpriseBagReservations_ConfirmationCode", "[ConfirmationCode] <> N''");
                    table.CheckConstraint("CK_SurpriseBagReservations_Quantity", "[Quantity] >= 1");
                    table.CheckConstraint("CK_SurpriseBagReservations_UnitPrice", "[UnitPrice] >= 0");
                    table.ForeignKey(
                        name: "FK_SurpriseBagReservations_SurpriseBags_SurpriseBagId",
                        column: x => x.SurpriseBagId,
                        principalTable: "SurpriseBags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SurpriseBagReservations_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SurpriseBagReservations_ConfirmationCode",
                table: "SurpriseBagReservations",
                column: "ConfirmationCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SurpriseBagReservations_SurpriseBagId_Status",
                table: "SurpriseBagReservations",
                columns: new[] { "SurpriseBagId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_SurpriseBagReservations_SurpriseBagId_UserId",
                table: "SurpriseBagReservations",
                columns: new[] { "SurpriseBagId", "UserId" },
                unique: true,
                filter: "[Status] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_SurpriseBagReservations_UserId",
                table: "SurpriseBagReservations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SurpriseBags_RestaurantId_PickupDate",
                table: "SurpriseBags",
                columns: new[] { "RestaurantId", "PickupDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SurpriseBagReservations");

            migrationBuilder.DropTable(
                name: "SurpriseBags");
        }
    }
}
