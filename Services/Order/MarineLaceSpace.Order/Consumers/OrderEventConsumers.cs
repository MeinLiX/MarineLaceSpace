using MarineLaceSpace.Enumerations;
using MarineLaceSpace.Interfaces.EventBus;
using MarineLaceSpace.Models.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Order.WebHost.Data;

namespace Order.WebHost.Consumers;

public static class OrderEventConsumers
{
    public static void ConfigureSubscriptions(IEventBus eventBus, IServiceProvider serviceProvider)
    {
        eventBus.Subscribe<BasketCheckoutEvent>(async (@event, ct) =>
        {
            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
            var bus = scope.ServiceProvider.GetService<IEventBus>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<OrderDbContext>>();

            var distinctShopIds = @event.Items
                .Where(i => !string.IsNullOrEmpty(i.ShopId))
                .Select(i => i.ShopId)
                .Distinct()
                .ToList();

            var order = new MarineLaceSpace.Models.Database.Order.Order
            {
                Id = Guid.NewGuid().ToString(),
                BuyerId = @event.BuyerId,
                BuyerEmail = @event.BuyerEmail,
                TotalPrice = @event.TotalPrice,
                StatusId = OrderStatus.New.Id,
                ShopId = distinctShopIds.Count == 1 ? distinctShopIds.First() : null,
                ShippingFullName = @event.ShippingAddress.FullName,
                ShippingAddressLine1 = @event.ShippingAddress.AddressLine1,
                ShippingAddressLine2 = @event.ShippingAddress.AddressLine2,
                ShippingCity = @event.ShippingAddress.City,
                ShippingPostalCode = @event.ShippingAddress.PostalCode,
                ShippingCountry = @event.ShippingAddress.Country,
                ShippingPhoneNumber = @event.ShippingAddress.PhoneNumber,
                Items = @event.Items.Select(i => new MarineLaceSpace.Models.Database.Order.OrderItem
                {
                    Id = Guid.NewGuid().ToString(),
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    SizeId = i.SizeId,
                    SizeName = i.SizeName,
                    ColorId = i.ColorId,
                    ColorName = i.ColorName,
                    MaterialId = i.MaterialId,
                    MaterialName = i.MaterialName,
                    UnitPrice = i.UnitPrice,
                    Quantity = i.Quantity,
                    Personalization = i.Personalization,
                    ImageUrl = i.ImageUrl
                }).ToList()
            };

            await db.Orders.AddAsync(order, ct);
            await db.SaveChangesAsync(ct);

            logger.LogInformation("Order {OrderId} created from basket checkout for buyer {BuyerId}", order.Id, order.BuyerId);

            if (bus != null)
            {
                await bus.PublishAsync(new OrderCreatedEvent
                {
                    OrderId = order.Id,
                    BuyerId = order.BuyerId,
                    BuyerEmail = order.BuyerEmail,
                    TotalPrice = order.TotalPrice
                }, ct);
            }
        });

        eventBus.Subscribe<PaymentSucceededEvent>(async (@event, ct) =>
        {
            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
            var bus = scope.ServiceProvider.GetService<IEventBus>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<OrderDbContext>>();

            var order = await db.Orders.FirstOrDefaultAsync(o => o.Id == @event.OrderId, ct);
            if (order != null)
            {
                var oldStatus = OrderStatus.FromId<OrderStatus>(order.StatusId)?.Name ?? "Unknown";
                order.StatusId = OrderStatus.Paid.Id;
                order.UpdatedAt = DateTime.UtcNow;
                await db.SaveChangesAsync(ct);
                logger.LogInformation("Order {OrderId} marked as Paid", order.Id);

                if (bus != null)
                {
                    await bus.PublishAsync(new OrderStatusChangedEvent
                    {
                        OrderId = order.Id,
                        BuyerId = order.BuyerId,
                        BuyerEmail = order.BuyerEmail,
                        OldStatus = oldStatus,
                        NewStatus = OrderStatus.Paid.Name
                    }, ct);
                }
            }
        });

        eventBus.Subscribe<PaymentFailedEvent>(async (@event, ct) =>
        {
            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<OrderDbContext>>();

            var order = await db.Orders.FirstOrDefaultAsync(o => o.Id == @event.OrderId, ct);
            if (order != null)
            {
                order.StatusId = OrderStatus.PaymentFailed.Id;
                order.UpdatedAt = DateTime.UtcNow;
                await db.SaveChangesAsync(ct);
                logger.LogInformation("Order {OrderId} marked as PaymentFailed: {Reason}", order.Id, @event.Reason);
            }
        });
    }
}
