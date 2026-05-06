namespace MarineLaceSpace.Models.Events;

/// <summary>
/// Published by the Catalog service after successfully reserving inventory for an order.
/// </summary>
public class InventoryReservedEvent : IntegrationEvent
{
    public string OrderId { get; set; } = string.Empty;
    public string BuyerId { get; set; } = string.Empty;
}
