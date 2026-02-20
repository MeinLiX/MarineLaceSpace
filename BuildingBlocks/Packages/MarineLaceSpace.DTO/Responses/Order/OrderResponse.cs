namespace MarineLaceSpace.DTO.Responses.Order;

public class OrderResponse
{
    public string Id { get; set; } = string.Empty;
    public string BuyerId { get; set; } = string.Empty;
    public string? BuyerEmail { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalPrice { get; set; }
    public string? TrackingNumber { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public ShippingAddressInfo ShippingAddress { get; set; } = new();
    public List<OrderItemResponse> Items { get; set; } = [];
}

public class ShippingAddressInfo
{
    public string FullName { get; set; } = string.Empty;
    public string AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
}

public class OrderItemResponse
{
    public string Id { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? SizeName { get; set; }
    public string? ColorName { get; set; }
    public string? MaterialName { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public string? Personalization { get; set; }
    public string? ImageUrl { get; set; }
}
