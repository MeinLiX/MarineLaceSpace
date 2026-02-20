using MarineLaceSpace.DTO.Common;

namespace MarineLaceSpace.Models.Events;

public class BasketCheckoutEvent : IntegrationEvent
{
    public string BuyerId { get; set; } = string.Empty;
    public string? BuyerEmail { get; set; }
    public decimal TotalPrice { get; set; }
    public ShippingAddressDto ShippingAddress { get; set; } = null!;
    public List<BasketCheckoutItem> Items { get; set; } = [];
}

public class BasketCheckoutItem
{
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? SizeId { get; set; }
    public string? SizeName { get; set; }
    public string? ColorId { get; set; }
    public string? ColorName { get; set; }
    public string? MaterialId { get; set; }
    public string? MaterialName { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public string? Personalization { get; set; }
    public string? ImageUrl { get; set; }
    public string? ShopId { get; set; }
}
