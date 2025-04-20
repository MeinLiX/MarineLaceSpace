using System.ComponentModel.DataAnnotations;

namespace MarineLaceSpace.Models.Database.Catalog;

// Персоналізація замовлення
public class ProductPersonalization
{
    [Key]
    public string Id { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; }

    [StringLength(512)]
    public string CustomText { get; set; }

    public string OrderId { get; set; }

    public string UserId { get; set; }
}
