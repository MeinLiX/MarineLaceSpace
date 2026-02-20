using BB.Common.Routes;
using MarineLaceSpace.DTO.Responses;
using MarineLaceSpace.Models.Routes;
using Notification.WebHost.Services;

namespace Notification.WebHost.Routes;

internal class NotificationHandlers
{
    private record NotificationServices : BasicRouteServices
    {
        public required ILogger<NotificationHandlers> Logger { get; init; }
        public required IEmailService EmailService { get; init; }
    }

    internal static Delegate SendEmailHandler =>
        async (SendEmailRequest request, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<NotificationServices>(sp, async (services) =>
            {
                services.Logger.LogInformation("Sending email to {To}: {Subject}", request.To, request.Subject);
                await services.EmailService.SendEmailAsync(request.To, request.Subject, request.Body);
                return Results.Ok(RESTResult.Success("Email sent (simulated)."));
            });

    internal static Delegate GetHubInfoHandler =>
        async (IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<NotificationServices>(sp, async (services) =>
            {
                await Task.CompletedTask;
                return Results.Ok(RESTResult<object>.Success(new
                {
                    HubUrl = "/api/notifications/hub",
                    SupportedEvents = new[] { "OrderCreated", "OrderStatusChanged", "PaymentSucceeded", "PaymentFailed" }
                }));
            });
}

public class SendEmailRequest
{
    public string To { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
}
