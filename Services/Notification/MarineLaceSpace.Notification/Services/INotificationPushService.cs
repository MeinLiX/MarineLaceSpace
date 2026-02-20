using Microsoft.AspNetCore.SignalR;
using Notification.WebHost.Hubs;

namespace Notification.WebHost.Services;

public interface INotificationPushService
{
    Task SendToUserAsync(string userId, string type, object payload);
    Task SendToGroupAsync(string groupName, string type, object payload);
}

public class SignalRNotificationPushService : INotificationPushService
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<SignalRNotificationPushService> _logger;

    public SignalRNotificationPushService(
        IHubContext<NotificationHub> hubContext,
        ILogger<SignalRNotificationPushService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task SendToUserAsync(string userId, string type, object payload)
    {
        _logger.LogInformation("Sending notification to user {UserId}: {Type}", userId, type);
        await _hubContext.Clients.Group($"user-{userId}").SendAsync("ReceiveNotification", new
        {
            Type = type,
            Payload = payload,
            Timestamp = DateTime.UtcNow
        });
    }

    public async Task SendToGroupAsync(string groupName, string type, object payload)
    {
        _logger.LogInformation("Sending notification to group {GroupName}: {Type}", groupName, type);
        await _hubContext.Clients.Group(groupName).SendAsync("ReceiveNotification", new
        {
            Type = type,
            Payload = payload,
            Timestamp = DateTime.UtcNow
        });
    }
}
