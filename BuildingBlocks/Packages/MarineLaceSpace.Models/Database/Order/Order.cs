using System.ComponentModel.DataAnnotations;

namespace MarineLaceSpace.Models.Database.Order;

public class Order
{
    [Key]
    public string Id { get; set; } = string.Empty;

    [Required]
    public string BuyerId { get; set; } = string.Empty;

    public string? BuyerEmail { get; set; }

    public string? ShopId { get; set; }

    public string ShippingFullName { get; set; } = string.Empty;
    public string ShippingAddressLine1 { get; set; } = string.Empty;
    public string? ShippingAddressLine2 { get; set; }
    public string ShippingCity { get; set; } = string.Empty;
    public string ShippingPostalCode { get; set; } = string.Empty;
    public string ShippingCountry { get; set; } = string.Empty;
    public string ShippingPhoneNumber { get; set; } = string.Empty;

    public int StatusId { get; set; } = 1; // New

    public decimal TotalPrice { get; set; }

    public string? TrackingNumber { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public virtual ICollection<OrderItem> Items { get; set; } = [];
}
