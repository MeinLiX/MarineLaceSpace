using MarineLaceSpace.Enumerations;
using System.ComponentModel.DataAnnotations;

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

    public ICollection<ProductSize> ProductSizes { get; set; } = [];
}
