using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarineLaceSpace.Catalog.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddShopIdToDictionaries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ShopId",
                table: "Sizes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShopId",
                table: "Materials",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShopId",
                table: "Colors",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sizes_ShopId",
                table: "Sizes",
                column: "ShopId");

            migrationBuilder.CreateIndex(
                name: "IX_Materials_ShopId",
                table: "Materials",
                column: "ShopId");

            migrationBuilder.CreateIndex(
                name: "IX_Colors_ShopId",
                table: "Colors",
                column: "ShopId");

            migrationBuilder.AddForeignKey(
                name: "FK_Colors_Shops_ShopId",
                table: "Colors",
                column: "ShopId",
                principalTable: "Shops",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Materials_Shops_ShopId",
                table: "Materials",
                column: "ShopId",
                principalTable: "Shops",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Sizes_Shops_ShopId",
                table: "Sizes",
                column: "ShopId",
                principalTable: "Shops",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Colors_Shops_ShopId",
                table: "Colors");

            migrationBuilder.DropForeignKey(
                name: "FK_Materials_Shops_ShopId",
                table: "Materials");

            migrationBuilder.DropForeignKey(
                name: "FK_Sizes_Shops_ShopId",
                table: "Sizes");

            migrationBuilder.DropIndex(
                name: "IX_Sizes_ShopId",
                table: "Sizes");

            migrationBuilder.DropIndex(
                name: "IX_Materials_ShopId",
                table: "Materials");

            migrationBuilder.DropIndex(
                name: "IX_Colors_ShopId",
                table: "Colors");

            migrationBuilder.DropColumn(
                name: "ShopId",
                table: "Sizes");

            migrationBuilder.DropColumn(
                name: "ShopId",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "ShopId",
                table: "Colors");
        }
    }
}
