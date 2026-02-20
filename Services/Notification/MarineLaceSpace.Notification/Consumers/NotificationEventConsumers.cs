using MarineLaceSpace.Interfaces.EventBus;
using MarineLaceSpace.Models.Events;

namespace Notification.WebHost.Consumers;

public static class NotificationEventConsumers
{
    public static void ConfigureSubscriptions(IEventBus eventBus, ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("NotificationConsumers");

        eventBus.Subscribe<UserRegisteredEvent>(async (@event, ct) =>
        {
            if (!@event.IsAnonymous)
            {
                logger.LogInformation("[EMAIL] Welcome email to {Email} for user {UserId}", @event.Email, @event.UserId);
                // In production: send via SMTP/SendGrid
            }
            await Task.CompletedTask;
        });

        eventBus.Subscribe<OrderCreatedEvent>(async (@event, ct) =>
        {
            logger.LogInformation("[EMAIL] Order confirmation to {Email} for order {OrderId}", @event.BuyerEmail, @event.OrderId);
            logger.LogInformation("[SIGNALR] Admin notification: new order {OrderId}", @event.OrderId);
            await Task.CompletedTask;
        });

        eventBus.Subscribe<OrderStatusChangedEvent>(async (@event, ct) =>
        {
            logger.LogInformation("[EMAIL] Order status update to {Email}: order {OrderId} changed from {OldStatus} to {NewStatus}",
                @event.BuyerEmail, @event.OrderId, @event.OldStatus, @event.NewStatus);
            logger.LogInformation("[SIGNALR] Push to buyer {BuyerId}: order {OrderId} is now {Status}",
                @event.BuyerId, @event.OrderId, @event.NewStatus);
            await Task.CompletedTask;
        });

        eventBus.Subscribe<PaymentSucceededEvent>(async (@event, ct) =>
        {
            logger.LogInformation("[EMAIL] Payment receipt to {Email} for payment {PaymentId}, order {OrderId}",
                @event.BuyerEmail, @event.PaymentId, @event.OrderId);
            await Task.CompletedTask;
        });

        eventBus.Subscribe<PaymentFailedEvent>(async (@event, ct) =>
        {
            logger.LogInformation("[EMAIL] Payment failed notification to {Email} for order {OrderId}: {Reason}",
                @event.BuyerEmail, @event.OrderId, @event.Reason);
            await Task.CompletedTask;
        });

        eventBus.Subscribe<PasswordResetRequestedEvent>(async (@event, ct) =>
        {
            logger.LogInformation("[EMAIL] Password reset link to {Email} with token {Token}", @event.Email, @event.ResetToken);
            await Task.CompletedTask;
        });
    }
}
