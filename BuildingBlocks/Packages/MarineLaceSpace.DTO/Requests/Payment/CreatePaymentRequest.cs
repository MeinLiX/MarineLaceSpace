namespace MarineLaceSpace.DTO.Requests.Payment;

public class CreatePaymentRequest
{
    public string OrderId { get; set; } = string.Empty;
    public int ProviderId { get; set; } = 1;
}
