using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EatKath.API.Migrations
{
    /// <inheritdoc />
    public partial class AddRestaurantIsDemo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDemo",
                table: "Restaurants",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDemo",
                table: "Restaurants");
        }
    }
}
