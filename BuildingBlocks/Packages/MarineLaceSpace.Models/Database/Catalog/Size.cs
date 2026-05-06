using MarineLaceSpace.Enumerations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarineLaceSpace.Models.Database.Catalog;

public class Size
{
    [Key]
    public string Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Name { get; set; }

    [StringLength(255)]
    public string Description { get; set; }

    public bool IsCustom { get; set; } = false;

    [StringLength(50)]
    public ProductSizeGender Gender { get; set; } = ProductSizeGender.Unisex;

    /// <summary>Null = global (admin-created). Set = shop-scoped.</summary>
    public string? ShopId { get; set; }

    [ForeignKey(nameof(ShopId))]
    public virtual Shop? Shop { get; set; }

    public ICollection<ProductSize> ProductSizes { get; set; } = [];
}
