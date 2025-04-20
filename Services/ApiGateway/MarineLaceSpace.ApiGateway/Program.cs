var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();


var app = builder.BuildWithPostActions();

app.MapGet("/test", (IHttpClientFactory httpClientFactory) =>
{
    var orderClient = httpClientFactory.CreateClient("order-api");
    var basketClient = httpClientFactory.CreateClient("basket-api");

    var orderBase = orderClient.BaseAddress?.ToString() ?? "null";
    var basketBase = basketClient.BaseAddress?.ToString() ?? "null";

    //await Task.WhenAll(orderRes, basketRes);

    return $"order BaseAddress: {orderBase}, basket BaseAddress: {basketBase}";
});

await app.RunAsync();
