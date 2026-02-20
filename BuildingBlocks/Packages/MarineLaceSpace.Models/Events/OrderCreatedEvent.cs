namespace MarineLaceSpace.Models.Events;

public class OrderCreatedEvent : IntegrationEvent
{
    public string OrderId { get; set; } = string.Empty;
    public string BuyerId { get; set; } = string.Empty;
    public string? BuyerEmail { get; set; }
    public decimal TotalPrice { get; set; }
}
