using System.ComponentModel.DataAnnotations;

namespace MarineLaceSpace.Models.Catalog;

// Персоналізація замовлення
public class ProductPersonalization
{
    public int Id { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; }

    [StringLength(512)]
    public string CustomText { get; set; }

    public string OrderId { get; set; }

    public string UserId { get; set; }
}
