namespace MarineLaceSpace.Models.Catalog;

// Зв'язкова таблиця між товаром і розміром з додатковою інформацією про ціну
public class ProductSize
{
    public int Id { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; }

    public int SizeId { get; set; }
    public Size Size { get; set; }

    // Базовий модифікатор ціни для цього розміру
    public decimal PriceModifier { get; set; } = 0;
}
