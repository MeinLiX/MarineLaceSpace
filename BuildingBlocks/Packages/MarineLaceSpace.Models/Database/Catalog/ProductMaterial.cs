using System.ComponentModel.DataAnnotations;

namespace MarineLaceSpace.Models.Database.Catalog;

public class ProductMaterial
{
    [Key]
    public string Id { get; set; }

    public string ProductId { get; set; }
    public Product Product { get; set; }

    public string MaterialId { get; set; }
    public Material Material { get; set; }

    public decimal PriceModifier { get; set; } = 1;
}
