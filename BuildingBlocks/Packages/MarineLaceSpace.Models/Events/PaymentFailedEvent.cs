namespace MarineLaceSpace.Models.Events;

public class PaymentFailedEvent : IntegrationEvent
{
    public string PaymentId { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string? BuyerEmail { get; set; }
}
