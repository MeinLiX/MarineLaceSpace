using BB.Common.EventBus;
using MarineLaceSpace.Interfaces.EventBus;
using Notification.WebHost.Consumers;
using Notification.WebHost.Hubs;
using Notification.WebHost.Routes;
using Notification.WebHost.Services;

var builder = WebApplication.CreateBuilder(args);

var rabbitConnectionString = builder.Configuration.GetConnectionString("rabbitmq");
if (!string.IsNullOrEmpty(rabbitConnectionString))
{
    builder.Services.AddRabbitMQEventBus(rabbitConnectionString, "notification-api");
}

builder.Services.AddSignalR();
builder.Services.AddSingleton<IEmailService, SmtpEmailService>();
builder.Services.AddSingleton<INotificationPushService, SignalRNotificationPushService>();

builder.AddServiceDefaults();

var app = builder.BuildWithPostActions();

var eventBus = app.Services.GetService<IEventBus>();
if (eventBus != null)
{
    NotificationEventConsumers.ConfigureSubscriptions(eventBus, app.Services);
}

app.MapHub<NotificationHub>("/api/notifications/hub");
app.MapNotificationRoutes();

await app.RunAsync();
