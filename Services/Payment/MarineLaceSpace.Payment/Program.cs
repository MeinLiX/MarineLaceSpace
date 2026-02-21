using BB.Common.EventBus;
using MarineLaceSpace.Interfaces.EventBus;
using Microsoft.EntityFrameworkCore;
using Payment.WebHost.Consumers;
using Payment.WebHost.Data;
using Payment.WebHost.Routes;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("pg-payment");
builder.Services.AddDbContext<PaymentDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

var rabbitConnectionString = builder.Configuration.GetConnectionString("rabbitmq");
if (!string.IsNullOrEmpty(rabbitConnectionString))
{
    builder.Services.AddRabbitMQEventBus(rabbitConnectionString, "payment-api");
}

builder.AddServiceDefaults();

var app = builder.BuildWithPostActions();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
    await db.Database.EnsureCreatedAsync();

    var eventBus = scope.ServiceProvider.GetService<IEventBus>();
    if (eventBus != null)
    {
        PaymentEventConsumers.ConfigureSubscriptions(eventBus, app.Services);
    }
}

app.MapPaymentRoutes();

await app.RunAsync();
