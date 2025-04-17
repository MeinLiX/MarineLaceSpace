using System.ComponentModel.DataAnnotations;

namespace MarineLaceSpace.Models.Catalog;

// Фотографії товару
public class ProductPhoto
{
    [Key]
    public string Id { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; }

    [Required]
    [StringLength(1000)]
    public string Url { get; set; }

    [StringLength(200)]
    public string Title { get; set; }

    public int SortOrder { get; set; } = 0;

    public string ProductSizeId { get; set; }
    public ProductSize ProductSize { get; set; }

    public string ProductColorId { get; set; }
    public ProductColor ProductColor { get; set; }

    public string ProductMaterialId { get; set; }
    public ProductMaterial ProductMaterial { get; set; }

    public bool IsMain { get; set; } = false;
}
