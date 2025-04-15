using System.ComponentModel.DataAnnotations;

namespace MarineLaceSpace.Models.Catalog;

// Довідник розмірів
public class Size
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Name { get; set; }

    [StringLength(255)]
    public string Description { get; set; }

    // Чи це кастомний розмір
    public bool IsCustom { get; set; }

    // Для якої статі цей розмір
    [StringLength(50)]
    public string Gender { get; set; } // "Man", "Woman", "Unisex"

    public ICollection<ProductSize> ProductSizes { get; set; }
}
