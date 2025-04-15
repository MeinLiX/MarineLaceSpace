using System.ComponentModel.DataAnnotations;

namespace MarineLaceSpace.Models.Catalog;

// Фотографії товару
public class ProductPhoto
{
    public int Id { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; }

    [Required]
    [StringLength(1000)]
    public string Url { get; set; }

    // Заголовок фото (опціонально)
    [StringLength(200)]
    public string Title { get; set; }

    // Порядок відображення
    public int SortOrder { get; set; } = 0;

    // Зв'язок з конкретними характеристиками (опціонально)
    public int? ProductSizeId { get; set; }
    public ProductSize ProductSize { get; set; }

    public int? ProductColorId { get; set; }
    public ProductColor ProductColor { get; set; }

    public int? ProductMaterialId { get; set; }
    public ProductMaterial ProductMaterial { get; set; }

    // Чи це головне фото товару
    public bool IsMain { get; set; } = false;
}
