namespace MarineLaceSpace.DTO.Requests.Payment;

public class RefundPaymentRequest
{
    public decimal? Amount { get; set; } // null = full refund
    public string? Reason { get; set; }
}
