using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EatKath.API.Migrations
{
    /// <inheritdoc />
    public partial class RefactorOfferModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RedemptionAmount",
                table: "Redemptions");

            migrationBuilder.DropColumn(
                name: "DiscountedPrice",
                table: "Deals");

            migrationBuilder.RenameColumn(
                name: "OriginalPrice",
                table: "Deals",
                newName: "DiscountPercentage");

            migrationBuilder.AddColumn<DateOnly>(
                name: "ArrivalDate",
                table: "Redemptions",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<TimeOnly>(
                name: "ArrivalTime",
                table: "Redemptions",
                type: "time",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));

            migrationBuilder.AddColumn<decimal>(
                name: "BillAmount",
                table: "Redemptions",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "Redemptions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountAmount",
                table: "Redemptions",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FinalAmount",
                table: "Redemptions",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GuestCount",
                table: "Redemptions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Redemptions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "StartDate",
                table: "Deals",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "EndDate",
                table: "Deals",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<int>(
                name: "AdvanceRedeemMinutes",
                table: "Deals",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DailyRedemptionLimit",
                table: "Deals",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "EndTime",
                table: "Deals",
                type: "time",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));

            migrationBuilder.AddColumn<bool>(
                name: "IsFeatured",
                table: "Deals",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MaximumGuests",
                table: "Deals",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OfferType",
                table: "Deals",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "StartTime",
                table: "Deals",
                type: "time",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArrivalDate",
                table: "Redemptions");

            migrationBuilder.DropColumn(
                name: "ArrivalTime",
                table: "Redemptions");

            migrationBuilder.DropColumn(
                name: "BillAmount",
                table: "Redemptions");

            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "Redemptions");

            migrationBuilder.DropColumn(
                name: "DiscountAmount",
                table: "Redemptions");

            migrationBuilder.DropColumn(
                name: "FinalAmount",
                table: "Redemptions");

            migrationBuilder.DropColumn(
                name: "GuestCount",
                table: "Redemptions");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Redemptions");

            migrationBuilder.DropColumn(
                name: "AdvanceRedeemMinutes",
                table: "Deals");

            migrationBuilder.DropColumn(
                name: "DailyRedemptionLimit",
                table: "Deals");

            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "Deals");

            migrationBuilder.DropColumn(
                name: "IsFeatured",
                table: "Deals");

            migrationBuilder.DropColumn(
                name: "MaximumGuests",
                table: "Deals");

            migrationBuilder.DropColumn(
                name: "OfferType",
                table: "Deals");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "Deals");

            migrationBuilder.RenameColumn(
                name: "DiscountPercentage",
                table: "Deals",
                newName: "OriginalPrice");

            migrationBuilder.AddColumn<decimal>(
                name: "RedemptionAmount",
                table: "Redemptions",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartDate",
                table: "Deals",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndDate",
                table: "Deals",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountedPrice",
                table: "Deals",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
