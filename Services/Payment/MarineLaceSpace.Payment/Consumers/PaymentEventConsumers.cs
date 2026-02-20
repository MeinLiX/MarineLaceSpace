using MarineLaceSpace.Enumerations;
using MarineLaceSpace.Interfaces.EventBus;
using MarineLaceSpace.Models.Database.Payment;
using MarineLaceSpace.Models.Events;
using Microsoft.Extensions.DependencyInjection;
using Payment.WebHost.Data;

namespace Payment.WebHost.Consumers;

public static class PaymentEventConsumers
{
    public static void ConfigureSubscriptions(IEventBus eventBus, IServiceProvider serviceProvider)
    {
        eventBus.Subscribe<OrderCreatedEvent>(async (@event, ct) =>
        {
            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<PaymentDbContext>>();

            var payment = new PaymentRecord
            {
                Id = Guid.NewGuid().ToString(),
                OrderId = @event.OrderId,
                Amount = @event.TotalPrice,
                StatusId = PaymentStatus.Pending.Id,
                BuyerEmail = @event.BuyerEmail
            };

            await db.Payments.AddAsync(payment, ct);
            await db.SaveChangesAsync(ct);
            logger.LogInformation("Pending payment {PaymentId} created for order {OrderId}", payment.Id, payment.OrderId);
        });
    }
}
