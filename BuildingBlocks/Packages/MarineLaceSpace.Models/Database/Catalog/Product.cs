using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarineLaceSpace.Models.Database.Catalog;

public class Product
{
    [Key]
    public string Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; }

    [StringLength(2000)]
    public string? Description { get; set; }

    [Required]
    public required string ShopId { get; set; }

    [ForeignKey("ShopId")]
    public virtual Shop Shop { get; set; }

    public string CategoryId { get; set; }

    [ForeignKey("CategoryId")]
    public virtual Category Category { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public bool AllowPersonalization { get; set; } = true;

    public bool IsActive { get; set; } = true;

    public virtual ICollection<ProductSize> AvailableSizes { get; set; } = [];
    public virtual ICollection<ProductColor> AvailableColors { get; set; } = [];
    public virtual ICollection<ProductMaterial> AvailableMaterials { get; set; } = [];
    public virtual ICollection<ProductPhoto> Photos { get; set; } = [];
    public virtual ICollection<ProductReview> Reviews { get; set; } = [];
    public virtual ICollection<ProductPrice> ProductPrices { get; set; } = [];
}