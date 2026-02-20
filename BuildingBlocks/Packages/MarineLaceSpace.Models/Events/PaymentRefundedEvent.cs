namespace MarineLaceSpace.Models.Events;

public class PaymentRefundedEvent : IntegrationEvent
{
    public string PaymentId { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
    public string RefundId { get; set; } = string.Empty;
    public decimal RefundAmount { get; set; }
    public string? BuyerEmail { get; set; }
    public string? Reason { get; set; }
}
