using BB.Common.EventBus;
using MarineLaceSpace.Interfaces.EventBus;
using Notification.WebHost.Consumers;
using Notification.WebHost.Routes;

var builder = WebApplication.CreateBuilder(args);

var rabbitConnectionString = builder.Configuration.GetConnectionString("rabbitmq");
if (!string.IsNullOrEmpty(rabbitConnectionString))
{
    builder.Services.AddRabbitMQEventBus(rabbitConnectionString, "notification-api");
}

builder.AddServiceDefaults();

var app = builder.BuildWithPostActions();

var eventBus = app.Services.GetService<IEventBus>();
if (eventBus != null)
{
    NotificationEventConsumers.ConfigureSubscriptions(eventBus, app.Services.GetRequiredService<ILoggerFactory>());
}

app.MapNotificationRoutes();

await app.RunAsync();
