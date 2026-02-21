using System.ComponentModel.DataAnnotations;

namespace MarineLaceSpace.Models.Database.Catalog;

public class ProductColor
{
    [Key]
    public string Id { get; set; }

    public string ProductId { get; set; }
    public Product Product { get; set; }

    public string ColorId { get; set; }
    public Color Color { get; set; }

    public decimal PriceModifier { get; set; } = 1;
}
