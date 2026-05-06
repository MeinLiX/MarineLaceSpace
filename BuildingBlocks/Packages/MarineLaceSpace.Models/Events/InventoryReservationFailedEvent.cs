namespace MarineLaceSpace.Models.Events;

/// <summary>
/// Published by the Catalog service when inventory reservation fails (insufficient stock).
/// </summary>
public class InventoryReservationFailedEvent : IntegrationEvent
{
    public string OrderId { get; set; } = string.Empty;
    public string BuyerId { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public List<FailedReservationItem> FailedItems { get; set; } = [];
}

public class FailedReservationItem
{
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int RequestedQuantity { get; set; }
    public int AvailableQuantity { get; set; }
}
