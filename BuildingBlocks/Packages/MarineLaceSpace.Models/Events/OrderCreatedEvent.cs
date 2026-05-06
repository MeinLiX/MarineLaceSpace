namespace MarineLaceSpace.Models.Events;

public class OrderCreatedEvent : IntegrationEvent
{
    public string OrderId { get; set; } = string.Empty;
    public string BuyerId { get; set; } = string.Empty;
    public string? BuyerEmail { get; set; }
    public decimal TotalPrice { get; set; }
    public List<OrderCreatedItem> Items { get; set; } = [];
}

public class OrderCreatedItem
{
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? SizeId { get; set; }
    public string? ColorId { get; set; }
    public string? MaterialId { get; set; }
    public int Quantity { get; set; }
}
