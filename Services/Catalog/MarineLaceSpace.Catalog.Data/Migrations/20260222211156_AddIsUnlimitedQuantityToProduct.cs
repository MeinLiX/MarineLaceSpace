using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarineLaceSpace.Catalog.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIsUnlimitedQuantityToProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsUnlimitedQuantity",
                table: "Products",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsUnlimitedQuantity",
                table: "Products");
        }
    }
}
