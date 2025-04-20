using System.ComponentModel.DataAnnotations;

namespace MarineLaceSpace.Models.Database.Catalog;

// Зв'язкова таблиця між товаром і розміром з додатковою інформацією про ціну
public class ProductSize
{
    [Key]
    public string Id { get; set; }

    public string ProductId { get; set; }
    public Product Product { get; set; }

    public string SizeId { get; set; }
    public Size Size { get; set; }

    public decimal PriceModifier { get; set; } = 0;
}
