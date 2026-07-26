using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EatKath.API.Migrations
{
    /// <inheritdoc />
    public partial class OfferModelRefactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFeatured",
                table: "Deals");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFeatured",
                table: "Deals",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
