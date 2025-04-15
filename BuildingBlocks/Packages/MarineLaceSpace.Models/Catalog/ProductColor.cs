namespace MarineLaceSpace.Models.Catalog;

// Зв'язкова таблиця між товаром і кольором
public class ProductColor
{
    public int Id { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; }

    public int ColorId { get; set; }
    public Color Color { get; set; }

    // Модифікатор ціни для цього кольору (опціонально)
    public decimal? PriceModifier { get; set; } = null;
}
