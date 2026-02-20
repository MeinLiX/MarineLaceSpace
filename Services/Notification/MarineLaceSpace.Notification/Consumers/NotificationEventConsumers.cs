using MarineLaceSpace.Interfaces.EventBus;
using MarineLaceSpace.Models.Events;
using Notification.WebHost.Services;

namespace Notification.WebHost.Consumers;

public static class NotificationEventConsumers
{
    public static void ConfigureSubscriptions(IEventBus eventBus, IServiceProvider serviceProvider)
    {
        eventBus.Subscribe<UserRegisteredEvent>(async (@event, ct) =>
        {
            using var scope = serviceProvider.CreateScope();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("NotificationConsumers");

            logger.LogInformation("User registered: {Email}, Anonymous: {IsAnon}", @event.Email, @event.IsAnonymous);

            if (!@event.IsAnonymous)
            {
                await emailService.SendTemplatedEmailAsync(@event.Email, "welcome", new Dictionary<string, string>
                {
                    ["userId"] = @event.UserId,
                    ["email"] = @event.Email
                });
            }
        });

        eventBus.Subscribe<OrderCreatedEvent>(async (@event, ct) =>
        {
            using var scope = serviceProvider.CreateScope();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
            var pushService = scope.ServiceProvider.GetRequiredService<INotificationPushService>();
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("NotificationConsumers");

            logger.LogInformation("Order created: {OrderId} for buyer {BuyerId}", @event.OrderId, @event.BuyerId);

            if (!string.IsNullOrEmpty(@event.BuyerEmail))
            {
                await emailService.SendEmailAsync(@event.BuyerEmail,
                    "Order Confirmation - MarineLaceSpace",
                    $"Your order #{@event.OrderId} has been placed. Total: {@event.TotalPrice:C}");
            }

            await pushService.SendToUserAsync(@event.BuyerId, "OrderCreated", new
            {
                @event.OrderId,
                @event.TotalPrice,
                Message = "Your order has been placed successfully!"
            });
        });

        eventBus.Subscribe<OrderStatusChangedEvent>(async (@event, ct) =>
        {
            using var scope = serviceProvider.CreateScope();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
            var pushService = scope.ServiceProvider.GetRequiredService<INotificationPushService>();
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("NotificationConsumers");

            logger.LogInformation("Order status changed: {OrderId} from {OldStatus} to {NewStatus}",
                @event.OrderId, @event.OldStatus, @event.NewStatus);

            if (!string.IsNullOrEmpty(@event.BuyerEmail))
            {
                await emailService.SendEmailAsync(@event.BuyerEmail,
                    $"Order Update - #{@event.OrderId}",
                    $"Your order status has changed from {@event.OldStatus} to {@event.NewStatus}.");
            }

            await pushService.SendToUserAsync(@event.BuyerId, "OrderStatusChanged", new
            {
                @event.OrderId,
                @event.OldStatus,
                @event.NewStatus,
                Message = $"Order status updated to {@event.NewStatus}"
            });

            await pushService.SendToGroupAsync($"order-{@event.OrderId}", "OrderStatusChanged", new
            {
                @event.OrderId,
                @event.NewStatus
            });
        });

        eventBus.Subscribe<PaymentSucceededEvent>(async (@event, ct) =>
        {
            using var scope = serviceProvider.CreateScope();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
            var pushService = scope.ServiceProvider.GetRequiredService<INotificationPushService>();
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("NotificationConsumers");

            logger.LogInformation("Payment succeeded: {PaymentId} for order {OrderId}", @event.PaymentId, @event.OrderId);

            if (!string.IsNullOrEmpty(@event.BuyerEmail))
            {
                await emailService.SendEmailAsync(@event.BuyerEmail,
                    "Payment Confirmed - MarineLaceSpace",
                    $"Payment of {@event.Amount:C} for order #{@event.OrderId} was successful.");
            }
        });

        eventBus.Subscribe<PaymentFailedEvent>(async (@event, ct) =>
        {
            using var scope = serviceProvider.CreateScope();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("NotificationConsumers");

            logger.LogInformation("Payment failed: {PaymentId} for order {OrderId}, Reason: {Reason}",
                @event.PaymentId, @event.OrderId, @event.Reason);

            if (!string.IsNullOrEmpty(@event.BuyerEmail))
            {
                await emailService.SendEmailAsync(@event.BuyerEmail,
                    "Payment Failed - MarineLaceSpace",
                    $"Payment for order #{@event.OrderId} failed. Reason: {@event.Reason}. Please try again.");
            }
        });

        eventBus.Subscribe<PasswordResetRequestedEvent>(async (@event, ct) =>
        {
            using var scope = serviceProvider.CreateScope();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("NotificationConsumers");

            logger.LogInformation("Password reset requested for: {Email}", @event.Email);

            await emailService.SendEmailAsync(@event.Email,
                "Password Reset - MarineLaceSpace",
                $"Your password reset token: {@event.ResetToken}. This token will expire soon.");
        });
    }
}
