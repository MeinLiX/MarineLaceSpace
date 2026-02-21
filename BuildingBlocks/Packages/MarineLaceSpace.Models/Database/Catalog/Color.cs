using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarineLaceSpace.Models.Database.Catalog;

// Довідник кольорів
public class Color
{
    [Key]
    public string Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Name { get; set; }

    [StringLength(9)]
    public string? HexCode { get; set; }

    public ICollection<ProductColor> ProductColors { get; set; } = [];
}
