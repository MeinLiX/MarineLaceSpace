using Basket.WebHost.Data;
using Basket.WebHost.Routes;
using BB.Common.EventBus;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("pg-basket");
builder.Services.AddDbContext<BasketDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

var rabbitConnectionString = builder.Configuration.GetConnectionString("rabbitmq");
if (!string.IsNullOrEmpty(rabbitConnectionString))
{
    builder.Services.AddRabbitMQEventBus(rabbitConnectionString, "basket-api");
}

builder.AddServiceDefaults();

var app = builder.BuildWithPostActions();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BasketDbContext>();
    await db.Database.MigrateAsync();
}

app.MapBasketRoutes();

await app.RunAsync();
