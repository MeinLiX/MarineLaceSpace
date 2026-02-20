namespace MarineLaceSpace.Models.Events;

public class OrderStatusChangedEvent : IntegrationEvent
{
    public string OrderId { get; set; } = string.Empty;
    public string BuyerId { get; set; } = string.Empty;
    public string? BuyerEmail { get; set; }
    public string OldStatus { get; set; } = string.Empty;
    public string NewStatus { get; set; } = string.Empty;
}
