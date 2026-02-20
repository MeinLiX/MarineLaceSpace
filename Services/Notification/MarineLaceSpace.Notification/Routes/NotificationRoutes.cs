namespace Notification.WebHost.Routes;

public static class NotificationRoutes
{
    public static void MapNotificationRoutes(this IEndpointRouteBuilder app)
    {
        var notificationsGroup = app.MapGroup("/api/notifications")
            .WithTags("Notifications");

        notificationsGroup.MapPost("/email", NotificationHandlers.SendEmailHandler)
            .WithSummary("Send an email notification")
            .RequireAuthorization("AdminOnly");

        notificationsGroup.MapGet("/hub/info", NotificationHandlers.GetHubInfoHandler)
            .WithSummary("Get SignalR hub connection info");
    }
}
