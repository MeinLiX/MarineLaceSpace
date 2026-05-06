namespace MarineLaceSpace.DTO.Requests.Order;

public class UpdateOrderStatusRequest
{
    public int StatusId { get; set; }
    public string? TrackingNumber { get; set; }
    public string? CancellationReason { get; set; }
}
