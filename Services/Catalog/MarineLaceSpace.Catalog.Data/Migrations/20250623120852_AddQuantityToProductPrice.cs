using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarineLaceSpace.Catalog.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddQuantityToProductPrice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "ProductPrices",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "ProductPrices");
        }
    }
}
