using System.ComponentModel.DataAnnotations;

namespace MarineLaceSpace.Models.Catalog;

// Прайс-лист для товару з врахуванням комбінацій
public class ProductPrice
{
    [Key]
    public string Id { get; set; }

    public string ProductId { get; set; }
    public Product Product { get; set; }

    public string ProductSizeId { get; set; }
    public ProductSize ProductSize { get; set; }

    public string ProductMaterialId { get; set; }
    public ProductMaterial ProductMaterial { get; set; }

    public string ProductColorId { get; set; }
    public ProductColor ProductColor { get; set; }

    // Базова ціна для цієї комбінації
    public decimal BasePrice { get; set; }

    // Стара ціна (опціонально, для розрахунку знижки)
    public decimal? OldPrice { get; set; }

    // Розмір знижки у відсотках (опціонально)
    public decimal DiscountPercentage { get; set; } = 1;
}
