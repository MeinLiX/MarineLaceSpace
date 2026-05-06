using MarineLaceSpace.Catalog.Data.DBContexts;
using MarineLaceSpace.Interfaces.EventBus;
using MarineLaceSpace.Models.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Catalog.WebHost.Consumers;

public static class CatalogEventConsumers
{
    public static void ConfigureSubscriptions(IEventBus eventBus, IServiceProvider serviceProvider)
    {
        eventBus.Subscribe<OrderCreatedEvent>(async (@event, ct) =>
        {
            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
            var bus = scope.ServiceProvider.GetService<IEventBus>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<CatalogDbContext>>();

            logger.LogInformation(
                "Processing inventory reservation for Order {OrderId}, {ItemCount} item(s)",
                @event.OrderId, @event.Items.Count);

            if (@event.Items.Count == 0)
            {
                logger.LogWarning("Order {OrderId} has no items to reserve", @event.OrderId);
                if (bus != null)
                {
                    await bus.PublishAsync(new InventoryReservedEvent
                    {
                        OrderId = @event.OrderId,
                        BuyerId = @event.BuyerId
                    }, ct);
                }
                return;
            }

            var failedItems = new List<FailedReservationItem>();

            await using var transaction = await db.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.Serializable, ct);

            try
            {
                var productIds = @event.Items.Select(i => i.ProductId).Distinct().ToList();
                var products = await db.Products
                    .Where(p => productIds.Contains(p.Id))
                    .AsTracking()
                    .ToListAsync(ct);

                var productLookup = products.ToDictionary(p => p.Id);

                var allPrices = await db.ProductPrices
                    .Where(pp => productIds.Contains(pp.ProductId))
                    .AsTracking()
                    .ToListAsync(ct);

                foreach (var item in @event.Items)
                {
                    if (!productLookup.TryGetValue(item.ProductId, out var product))
                    {
                        failedItems.Add(new FailedReservationItem
                        {
                            ProductId = item.ProductId,
                            ProductName = item.ProductName,
                            RequestedQuantity = item.Quantity,
                            AvailableQuantity = 0
                        });
                        continue;
                    }

                    if (product.IsUnlimitedQuantity)
                    {
                        logger.LogInformation(
                            "Product {ProductId} ({ProductName}) is unlimited quantity, skipping deduction",
                            item.ProductId, item.ProductName);
                        continue;
                    }

                    var priceEntry = allPrices.FirstOrDefault(pp =>
                        pp.ProductId == item.ProductId &&
                        pp.ProductSizeId == item.SizeId &&
                        pp.ProductColorId == item.ColorId &&
                        pp.ProductMaterialId == item.MaterialId);

                    if (priceEntry == null)
                    {
                        failedItems.Add(new FailedReservationItem
                        {
                            ProductId = item.ProductId,
                            ProductName = item.ProductName,
                            RequestedQuantity = item.Quantity,
                            AvailableQuantity = 0
                        });
                        continue;
                    }

                    if (priceEntry.Quantity < item.Quantity)
                    {
                        failedItems.Add(new FailedReservationItem
                        {
                            ProductId = item.ProductId,
                            ProductName = item.ProductName,
                            RequestedQuantity = item.Quantity,
                            AvailableQuantity = priceEntry.Quantity
                        });
                        continue;
                    }

                    priceEntry.Quantity -= item.Quantity;
                    logger.LogInformation(
                        "Reserved {Qty} of Product {ProductId} (variant: size={SizeId}, color={ColorId}, material={MaterialId}). Remaining: {Remaining}",
                        item.Quantity, item.ProductId, item.SizeId, item.ColorId, item.MaterialId, priceEntry.Quantity);
                }

                if (failedItems.Count > 0)
                {
                    await transaction.RollbackAsync(ct);

                    var failedNames = string.Join(", ", failedItems.Select(f => $"{f.ProductName} (need {f.RequestedQuantity}, have {f.AvailableQuantity})"));
                    logger.LogWarning(
                        "Inventory reservation FAILED for Order {OrderId}: {FailedItems}",
                        @event.OrderId, failedNames);

                    if (bus != null)
                    {
                        await bus.PublishAsync(new InventoryReservationFailedEvent
                        {
                            OrderId = @event.OrderId,
                            BuyerId = @event.BuyerId,
                            Reason = $"Insufficient stock for: {failedNames}",
                            FailedItems = failedItems
                        }, ct);
                    }
                    return;
                }

                await db.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);

                logger.LogInformation("Inventory reservation SUCCEEDED for Order {OrderId}", @event.OrderId);

                if (bus != null)
                {
                    await bus.PublishAsync(new InventoryReservedEvent
                    {
                        OrderId = @event.OrderId,
                        BuyerId = @event.BuyerId
                    }, ct);
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(ct);
                logger.LogError(ex, "Inventory reservation ERROR for Order {OrderId}", @event.OrderId);

                if (bus != null)
                {
                    await bus.PublishAsync(new InventoryReservationFailedEvent
                    {
                        OrderId = @event.OrderId,
                        BuyerId = @event.BuyerId,
                        Reason = $"Internal error: {ex.Message}",
                        FailedItems = []
                    }, ct);
                }
            }
        });
    }
}
