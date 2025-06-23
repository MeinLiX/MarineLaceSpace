using System.ComponentModel.DataAnnotations;

namespace MarineLaceSpace.Models.Database.Catalog;

public class ProductPhoto
{
    [Key]
    public string Id { get; set; }

    public string ProductId { get; set; }
    public virtual Product Product { get; set; }

    [Required]
    [StringLength(1000)]
    public string Url { get; set; }

    [StringLength(200)]
    public string? Title { get; set; }

    public int SortOrder { get; set; } = 0;

    public string? ProductSizeId { get; set; }
    public virtual ProductSize? ProductSize { get; set; }

    public string? ProductColorId { get; set; }
    public virtual ProductColor? ProductColor { get; set; }

    public string? ProductMaterialId { get; set; }
    public virtual ProductMaterial? ProductMaterial { get; set; }

    public bool IsMain { get; set; } = false; 
}