using Basket.WebHost.Data;
using Basket.WebHost.Routes;
using BB.Common.EventBus;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("redis");
});

builder.Services.AddSingleton<IBasketRepository, RedisBasketRepository>();

builder.Services.AddRabbitMQEventBus(
    builder.Configuration.GetConnectionString("rabbitmq") ?? throw new InvalidOperationException("RabbitMQ connection string missing"),
    "basket-api");

var app = builder.BuildWithPostActions();

app.MapBasketRoutes();

await app.RunAsync();
