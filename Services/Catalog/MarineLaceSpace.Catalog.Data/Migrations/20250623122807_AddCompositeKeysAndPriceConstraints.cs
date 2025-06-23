using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarineLaceSpace.Catalog.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCompositeKeysAndPriceConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductPhotos_ProductColors_ProductColorId",
                table: "ProductPhotos");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductPhotos_ProductMaterials_ProductMaterialId",
                table: "ProductPhotos");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductPhotos_ProductSizes_ProductSizeId",
                table: "ProductPhotos");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductPrices_ProductColors_ProductColorId",
                table: "ProductPrices");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductPrices_ProductMaterials_ProductMaterialId",
                table: "ProductPrices");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductPrices_ProductSizes_ProductSizeId",
                table: "ProductPrices");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductSizes",
                table: "ProductSizes");

            migrationBuilder.DropIndex(
                name: "IX_ProductSizes_ProductId",
                table: "ProductSizes");

            migrationBuilder.DropIndex(
                name: "IX_ProductPrices_ProductColorId",
                table: "ProductPrices");

            migrationBuilder.DropIndex(
                name: "IX_ProductPrices_ProductId",
                table: "ProductPrices");

            migrationBuilder.DropIndex(
                name: "IX_ProductPrices_ProductMaterialId",
                table: "ProductPrices");

            migrationBuilder.DropIndex(
                name: "IX_ProductPrices_ProductSizeId",
                table: "ProductPrices");

            migrationBuilder.DropIndex(
                name: "IX_ProductPhotos_ProductColorId",
                table: "ProductPhotos");

            migrationBuilder.DropIndex(
                name: "IX_ProductPhotos_ProductMaterialId",
                table: "ProductPhotos");

            migrationBuilder.DropIndex(
                name: "IX_ProductPhotos_ProductSizeId",
                table: "ProductPhotos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductMaterials",
                table: "ProductMaterials");

            migrationBuilder.DropIndex(
                name: "IX_ProductMaterials_ProductId",
                table: "ProductMaterials");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductColors",
                table: "ProductColors");

            migrationBuilder.DropIndex(
                name: "IX_ProductColors_ProductId",
                table: "ProductColors");

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "ProductSizes",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<decimal>(
                name: "OldPrice",
                table: "ProductPrices",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "BasePrice",
                table: "ProductPrices",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AddColumn<string>(
                name: "ProductColorColorId",
                table: "ProductPrices",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductColorProductId",
                table: "ProductPrices",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductMaterialMaterialId",
                table: "ProductPrices",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductMaterialProductId",
                table: "ProductPrices",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductSizeProductId",
                table: "ProductPrices",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductSizeSizeId",
                table: "ProductPrices",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductColorColorId",
                table: "ProductPhotos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductColorProductId",
                table: "ProductPhotos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductMaterialMaterialId",
                table: "ProductPhotos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductMaterialProductId",
                table: "ProductPhotos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductSizeProductId",
                table: "ProductPhotos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductSizeSizeId",
                table: "ProductPhotos",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "ProductMaterials",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "ProductColors",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductSizes",
                table: "ProductSizes",
                columns: new[] { "ProductId", "SizeId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductMaterials",
                table: "ProductMaterials",
                columns: new[] { "ProductId", "MaterialId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductColors",
                table: "ProductColors",
                columns: new[] { "ProductId", "ColorId" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductPrices_ProductColorProductId_ProductColorColorId",
                table: "ProductPrices",
                columns: new[] { "ProductColorProductId", "ProductColorColorId" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductPrices_ProductId_ProductSizeId_ProductColorId_Produc~",
                table: "ProductPrices",
                columns: new[] { "ProductId", "ProductSizeId", "ProductColorId", "ProductMaterialId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductPrices_ProductMaterialProductId_ProductMaterialMater~",
                table: "ProductPrices",
                columns: new[] { "ProductMaterialProductId", "ProductMaterialMaterialId" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductPrices_ProductSizeProductId_ProductSizeSizeId",
                table: "ProductPrices",
                columns: new[] { "ProductSizeProductId", "ProductSizeSizeId" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductPhotos_ProductColorProductId_ProductColorColorId",
                table: "ProductPhotos",
                columns: new[] { "ProductColorProductId", "ProductColorColorId" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductPhotos_ProductMaterialProductId_ProductMaterialMater~",
                table: "ProductPhotos",
                columns: new[] { "ProductMaterialProductId", "ProductMaterialMaterialId" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductPhotos_ProductSizeProductId_ProductSizeSizeId",
                table: "ProductPhotos",
                columns: new[] { "ProductSizeProductId", "ProductSizeSizeId" });

            migrationBuilder.AddForeignKey(
                name: "FK_ProductPhotos_ProductColors_ProductColorProductId_ProductCo~",
                table: "ProductPhotos",
                columns: new[] { "ProductColorProductId", "ProductColorColorId" },
                principalTable: "ProductColors",
                principalColumns: new[] { "ProductId", "ColorId" });

            migrationBuilder.AddForeignKey(
                name: "FK_ProductPhotos_ProductMaterials_ProductMaterialProductId_Pro~",
                table: "ProductPhotos",
                columns: new[] { "ProductMaterialProductId", "ProductMaterialMaterialId" },
                principalTable: "ProductMaterials",
                principalColumns: new[] { "ProductId", "MaterialId" });

            migrationBuilder.AddForeignKey(
                name: "FK_ProductPhotos_ProductSizes_ProductSizeProductId_ProductSize~",
                table: "ProductPhotos",
                columns: new[] { "ProductSizeProductId", "ProductSizeSizeId" },
                principalTable: "ProductSizes",
                principalColumns: new[] { "ProductId", "SizeId" });

            migrationBuilder.AddForeignKey(
                name: "FK_ProductPrices_ProductColors_ProductColorProductId_ProductCo~",
                table: "ProductPrices",
                columns: new[] { "ProductColorProductId", "ProductColorColorId" },
                principalTable: "ProductColors",
                principalColumns: new[] { "ProductId", "ColorId" });

            migrationBuilder.AddForeignKey(
                name: "FK_ProductPrices_ProductMaterials_ProductMaterialProductId_Pro~",
                table: "ProductPrices",
                columns: new[] { "ProductMaterialProductId", "ProductMaterialMaterialId" },
                principalTable: "ProductMaterials",
                principalColumns: new[] { "ProductId", "MaterialId" });

            migrationBuilder.AddForeignKey(
                name: "FK_ProductPrices_ProductSizes_ProductSizeProductId_ProductSize~",
                table: "ProductPrices",
                columns: new[] { "ProductSizeProductId", "ProductSizeSizeId" },
                principalTable: "ProductSizes",
                principalColumns: new[] { "ProductId", "SizeId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductPhotos_ProductColors_ProductColorProductId_ProductCo~",
                table: "ProductPhotos");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductPhotos_ProductMaterials_ProductMaterialProductId_Pro~",
                table: "ProductPhotos");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductPhotos_ProductSizes_ProductSizeProductId_ProductSize~",
                table: "ProductPhotos");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductPrices_ProductColors_ProductColorProductId_ProductCo~",
                table: "ProductPrices");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductPrices_ProductMaterials_ProductMaterialProductId_Pro~",
                table: "ProductPrices");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductPrices_ProductSizes_ProductSizeProductId_ProductSize~",
                table: "ProductPrices");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductSizes",
                table: "ProductSizes");

            migrationBuilder.DropIndex(
                name: "IX_ProductPrices_ProductColorProductId_ProductColorColorId",
                table: "ProductPrices");

            migrationBuilder.DropIndex(
                name: "IX_ProductPrices_ProductId_ProductSizeId_ProductColorId_Produc~",
                table: "ProductPrices");

            migrationBuilder.DropIndex(
                name: "IX_ProductPrices_ProductMaterialProductId_ProductMaterialMater~",
                table: "ProductPrices");

            migrationBuilder.DropIndex(
                name: "IX_ProductPrices_ProductSizeProductId_ProductSizeSizeId",
                table: "ProductPrices");

            migrationBuilder.DropIndex(
                name: "IX_ProductPhotos_ProductColorProductId_ProductColorColorId",
                table: "ProductPhotos");

            migrationBuilder.DropIndex(
                name: "IX_ProductPhotos_ProductMaterialProductId_ProductMaterialMater~",
                table: "ProductPhotos");

            migrationBuilder.DropIndex(
                name: "IX_ProductPhotos_ProductSizeProductId_ProductSizeSizeId",
                table: "ProductPhotos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductMaterials",
                table: "ProductMaterials");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductColors",
                table: "ProductColors");

            migrationBuilder.DropColumn(
                name: "ProductColorColorId",
                table: "ProductPrices");

            migrationBuilder.DropColumn(
                name: "ProductColorProductId",
                table: "ProductPrices");

            migrationBuilder.DropColumn(
                name: "ProductMaterialMaterialId",
                table: "ProductPrices");

            migrationBuilder.DropColumn(
                name: "ProductMaterialProductId",
                table: "ProductPrices");

            migrationBuilder.DropColumn(
                name: "ProductSizeProductId",
                table: "ProductPrices");

            migrationBuilder.DropColumn(
                name: "ProductSizeSizeId",
                table: "ProductPrices");

            migrationBuilder.DropColumn(
                name: "ProductColorColorId",
                table: "ProductPhotos");

            migrationBuilder.DropColumn(
                name: "ProductColorProductId",
                table: "ProductPhotos");

            migrationBuilder.DropColumn(
                name: "ProductMaterialMaterialId",
                table: "ProductPhotos");

            migrationBuilder.DropColumn(
                name: "ProductMaterialProductId",
                table: "ProductPhotos");

            migrationBuilder.DropColumn(
                name: "ProductSizeProductId",
                table: "ProductPhotos");

            migrationBuilder.DropColumn(
                name: "ProductSizeSizeId",
                table: "ProductPhotos");

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "ProductSizes",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "OldPrice",
                table: "ProductPrices",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "BasePrice",
                table: "ProductPrices",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "ProductMaterials",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "ProductColors",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductSizes",
                table: "ProductSizes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductMaterials",
                table: "ProductMaterials",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductColors",
                table: "ProductColors",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_ProductSizes_ProductId",
                table: "ProductSizes",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductPrices_ProductColorId",
                table: "ProductPrices",
                column: "ProductColorId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductPrices_ProductId",
                table: "ProductPrices",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductPrices_ProductMaterialId",
                table: "ProductPrices",
                column: "ProductMaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductPrices_ProductSizeId",
                table: "ProductPrices",
                column: "ProductSizeId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductPhotos_ProductColorId",
                table: "ProductPhotos",
                column: "ProductColorId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductPhotos_ProductMaterialId",
                table: "ProductPhotos",
                column: "ProductMaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductPhotos_ProductSizeId",
                table: "ProductPhotos",
                column: "ProductSizeId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductMaterials_ProductId",
                table: "ProductMaterials",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductColors_ProductId",
                table: "ProductColors",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductPhotos_ProductColors_ProductColorId",
                table: "ProductPhotos",
                column: "ProductColorId",
                principalTable: "ProductColors",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductPhotos_ProductMaterials_ProductMaterialId",
                table: "ProductPhotos",
                column: "ProductMaterialId",
                principalTable: "ProductMaterials",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductPhotos_ProductSizes_ProductSizeId",
                table: "ProductPhotos",
                column: "ProductSizeId",
                principalTable: "ProductSizes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductPrices_ProductColors_ProductColorId",
                table: "ProductPrices",
                column: "ProductColorId",
                principalTable: "ProductColors",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductPrices_ProductMaterials_ProductMaterialId",
                table: "ProductPrices",
                column: "ProductMaterialId",
                principalTable: "ProductMaterials",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductPrices_ProductSizes_ProductSizeId",
                table: "ProductPrices",
                column: "ProductSizeId",
                principalTable: "ProductSizes",
                principalColumn: "Id");
        }
    }
}
