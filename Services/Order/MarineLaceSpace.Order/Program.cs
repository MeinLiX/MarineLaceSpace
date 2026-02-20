using BB.Common.EventBus;
using MarineLaceSpace.Interfaces.EventBus;
using Microsoft.EntityFrameworkCore;
using Order.WebHost.Consumers;
using Order.WebHost.Data;
using Order.WebHost.Routes;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("pg-order");
builder.Services.AddDbContext<OrderDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

var rabbitConnectionString = builder.Configuration.GetConnectionString("rabbitmq");
if (!string.IsNullOrEmpty(rabbitConnectionString))
{
    builder.Services.AddRabbitMQEventBus(rabbitConnectionString, "order-api");
}

builder.AddServiceDefaults();

var app = builder.BuildWithPostActions();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
    await db.Database.MigrateAsync();

    var eventBus = scope.ServiceProvider.GetService<IEventBus>();
    if (eventBus != null)
    {
        OrderEventConsumers.ConfigureSubscriptions(eventBus, app.Services);
    }
}

app.MapOrderRoutes();

await app.RunAsync();
