using System.ComponentModel.DataAnnotations;

namespace MarineLaceSpace.Models.Catalog;

// Довідник кольорів
public class Color
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Name { get; set; }

    [StringLength(20)]
    public string HexCode { get; set; }

    public ICollection<ProductColor> ProductColors { get; set; }
}
