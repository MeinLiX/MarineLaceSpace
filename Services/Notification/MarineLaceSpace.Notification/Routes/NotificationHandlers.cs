using BB.Common.Routes;
using MarineLaceSpace.DTO.Responses;
using MarineLaceSpace.Models.Routes;

namespace Notification.WebHost.Routes;

internal class NotificationHandlers
{
    private record NotificationServices : BasicRouteServices
    {
        public required ILogger<NotificationHandlers> Logger { get; init; }
    }

    internal static Delegate SendEmailHandler =>
        async (SendEmailRequest request, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<NotificationServices>(sp, async (services) =>
            {
                services.Logger.LogInformation("Sending email to {To}: {Subject}", request.To, request.Subject);
                // In production: integrate with SMTP/SendGrid
                await Task.CompletedTask;
                return Results.Ok(RESTResult.Success("Email sent (simulated)."));
            });
}

public class SendEmailRequest
{
    public string To { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
}
