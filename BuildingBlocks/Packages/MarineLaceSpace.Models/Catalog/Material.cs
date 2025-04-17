using System.ComponentModel.DataAnnotations;

namespace MarineLaceSpace.Models.Catalog;

// Довідник матеріалів/тканин
public class Material
{
    [Key]
    public string Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; }

    [StringLength(500)]
    public string Description { get; set; }

    public ICollection<ProductMaterial> ProductMaterials { get; set; } = [];
}
