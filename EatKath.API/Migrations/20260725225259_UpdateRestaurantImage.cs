using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EatKath.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRestaurantImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "RestaurantImages");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "RestaurantImages");

            migrationBuilder.RenameColumn(
                name: "IsPrimary",
                table: "RestaurantImages",
                newName: "IsLogo");

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "RestaurantImages",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "RestaurantImages");

            migrationBuilder.RenameColumn(
                name: "IsLogo",
                table: "RestaurantImages",
                newName: "IsPrimary");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "RestaurantImages",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "RestaurantImages",
                type: "datetime2",
                nullable: true);
        }
    }
}
