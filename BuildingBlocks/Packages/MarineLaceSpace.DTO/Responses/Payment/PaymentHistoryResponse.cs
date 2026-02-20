namespace MarineLaceSpace.DTO.Responses.Payment;

public class PaymentHistoryResponse
{
    public string PaymentId { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<StatusChangeEntry> StatusChanges { get; set; } = [];
    public List<RefundEntry> Refunds { get; set; } = [];
}

public class StatusChangeEntry
{
    public string OldStatus { get; set; } = string.Empty;
    public string NewStatus { get; set; } = string.Empty;
    public string? Note { get; set; }
    public DateTime ChangedAt { get; set; }
}

public class RefundEntry
{
    public string Id { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Reason { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
