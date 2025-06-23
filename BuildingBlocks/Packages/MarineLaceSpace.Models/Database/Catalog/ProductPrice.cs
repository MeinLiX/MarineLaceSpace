using System.ComponentModel.DataAnnotations;

namespace MarineLaceSpace.Models.Database.Catalog;

public class ProductPrice
{
    [Key]
    public string Id { get; set; }

    public string ProductId { get; set; }
    public virtual Product Product { get; set; }

    public string? ProductSizeId { get; set; }
    public virtual ProductSize? ProductSize { get; set; }

    public string? ProductMaterialId { get; set; }
    public virtual ProductMaterial? ProductMaterial { get; set; }

    public string? ProductColorId { get; set; }
    public virtual ProductColor? ProductColor { get; set; }

    public decimal BasePrice { get; set; }

    [Required]
    public int Quantity { get; set; }

    public decimal? OldPrice { get; set; }

    public decimal DiscountPercentage { get; set; } = 1;
}