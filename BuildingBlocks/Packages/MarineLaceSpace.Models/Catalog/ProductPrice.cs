namespace MarineLaceSpace.Models.Catalog;

// Прайс-лист для товару з врахуванням комбінацій
public class ProductPrice
{
    public int Id { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; }

    public int? ProductSizeId { get; set; }
    public ProductSize ProductSize { get; set; }

    public int? ProductMaterialId { get; set; }
    public ProductMaterial ProductMaterial { get; set; }

    public int? ProductColorId { get; set; }
    public ProductColor ProductColor { get; set; }

    // Базова ціна для цієї комбінації
    public decimal BasePrice { get; set; }

    // Стара ціна (опціонально, для розрахунку знижки)
    public decimal? OldPrice { get; set; }

    // Розмір знижки у відсотках (опціонально)
    public decimal? DiscountPercentage { get; set; }
}
