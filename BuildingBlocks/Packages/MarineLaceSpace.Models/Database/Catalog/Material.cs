using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarineLaceSpace.Models.Database.Catalog;

public class Material
{
    [Key]
    public string Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; }

    [StringLength(500)]
    public string Description { get; set; }

    [StringLength(1000)]
    public string? ImageUrl { get; set; }

    /// <summary>Null = global (admin-created). Set = shop-scoped.</summary>
    public string? ShopId { get; set; }

    [ForeignKey(nameof(ShopId))]
    public virtual Shop? Shop { get; set; }

    public ICollection<ProductMaterial> ProductMaterials { get; set; } = [];
}
