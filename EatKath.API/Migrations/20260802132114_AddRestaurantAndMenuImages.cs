using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EatKath.API.Migrations
{
    /// <inheritdoc />
    public partial class AddRestaurantAndMenuImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsLogo",
                table: "RestaurantImages",
                newName: "IsPrimary");

            migrationBuilder.AddColumn<string>(
                name: "CoverImageUrl",
                table: "Restaurants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MenuPdfUrl",
                table: "Restaurants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Caption",
                table: "RestaurantImages",
                type: "nvarchar(max)",
                nullable: true);

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

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "MenuItems",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CoverImageUrl",
                table: "Restaurants");

            migrationBuilder.DropColumn(
                name: "MenuPdfUrl",
                table: "Restaurants");

            migrationBuilder.DropColumn(
                name: "Caption",
                table: "RestaurantImages");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "RestaurantImages");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "RestaurantImages");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "MenuItems");

            migrationBuilder.RenameColumn(
                name: "IsPrimary",
                table: "RestaurantImages",
                newName: "IsLogo");
        }
    }
}
