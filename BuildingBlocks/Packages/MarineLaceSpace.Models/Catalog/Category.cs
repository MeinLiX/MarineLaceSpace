using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarineLaceSpace.Models.Catalog;

public class Category
{
    [Key]
    public string Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; }

    [StringLength(500)]
    public string Description { get; set; }

    // Самореферентна зв'язка для побудови дерева категорій
    public int? ParentCategoryId { get; set; }

    [ForeignKey("ParentCategoryId")]
    public Category ParentCategory { get; set; }

    public ICollection<Category> Subcategories { get; set; } = [];
    public ICollection<Product> Products { get; set; } = [];

    // Рівень вкладеності (для швидкого доступу)
    public int Level { get; set; } = 0;

    // Повний шлях категорії (для швидкого пошуку)
    [StringLength(1000)]
    public string FullPath { get; set; }
}