namespace MarineLaceSpace.Models.Events;

public class PaymentSucceededEvent : IntegrationEvent
{
    public string PaymentId { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? BuyerEmail { get; set; }
}
