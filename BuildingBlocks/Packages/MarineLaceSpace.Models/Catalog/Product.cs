using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarineLaceSpace.Models.Catalog;

// Основна таблиця товарів
public class Product
{
    [Key]
    public string Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; }

    [StringLength(2000)]
    public string Description { get; set; }

    public string CategoryId { get; set; }

    [ForeignKey("CategoryId")]
    public Category Category { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public bool AllowPersonalization { get; set; } = true;

    public bool IsActive { get; set; } = true;

    // Навігаційні властивості
    public ICollection<ProductSize> AvailableSizes { get; set; } = [];
    public ICollection<ProductColor> AvailableColors { get; set; } = [];
    public ICollection<ProductMaterial> AvailableMaterials { get; set; } = [];
    public ICollection<ProductPhoto> Photos { get; set; } = [];
    public ICollection<ProductReview> Reviews { get; set; } = [];
}
