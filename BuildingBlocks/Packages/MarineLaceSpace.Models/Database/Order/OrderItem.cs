using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarineLaceSpace.Models.Database.Order;

public class OrderItem
{
    [Key]
    public string Id { get; set; } = string.Empty;

    [Required]
    public string OrderId { get; set; } = string.Empty;

    [ForeignKey("OrderId")]
    public virtual Order Order { get; set; } = null!;

    public string ProductId { get; set; } = string.Empty;

    [StringLength(200)]
    public string ProductName { get; set; } = string.Empty;

    public string? SizeId { get; set; }
    public string? SizeName { get; set; }
    public string? ColorId { get; set; }
    public string? ColorName { get; set; }
    public string? MaterialId { get; set; }
    public string? MaterialName { get; set; }

    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }

    [StringLength(512)]
    public string? Personalization { get; set; }

    public string? ImageUrl { get; set; }
}
