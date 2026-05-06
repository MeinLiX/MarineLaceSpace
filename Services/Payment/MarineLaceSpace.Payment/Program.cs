using BB.Common.EventBus;
using BB.Common.Extensions;
using MarineLaceSpace.Interfaces.EventBus;
using Microsoft.EntityFrameworkCore;
using Payment.WebHost.Consumers;
using Payment.WebHost.Data;
using Payment.WebHost.Routes;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("pg-payment");
builder.Services.AddDbContext<PaymentDbContext>(options =>
{
    options.UseNpgsql(connectionString, npgsqlOptions =>
        npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(30), errorCodesToAdd: null));
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
    await scope.ServiceProvider.EnsureCreatedWithRetryAsync<PaymentDbContext>();

    var eventBus = scope.ServiceProvider.GetService<IEventBus>();
    if (eventBus != null)
    {
        PaymentEventConsumers.ConfigureSubscriptions(eventBus, app.Services);
    }
}

app.MapPaymentRoutes();

await app.RunAsync();
