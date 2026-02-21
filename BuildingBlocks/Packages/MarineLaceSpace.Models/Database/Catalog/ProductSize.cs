using System.ComponentModel.DataAnnotations;

namespace MarineLaceSpace.Models.Database.Catalog;

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
