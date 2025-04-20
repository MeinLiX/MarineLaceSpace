var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();


var app = builder.BuildWithPostActions();


app.MapGet("/test", async (IHttpClientFactory httpClientFactory, IConfiguration cfg) =>
{
    var orderClient = httpClientFactory.CreateClient("order-api");
    var basketClient = httpClientFactory.CreateClient("basket-api");

    var orderRes = orderClient.GetStringAsync("/health");
    var basketRes = basketClient.GetStringAsync("/health");

    await Task.WhenAll(orderRes, basketRes);

    return $"Health. Order '{orderRes.Result}'; Basket: '{basketRes.Result}'";
}).WithDescription("Test 'Order api' and 'Basket api' connection.");

await app.RunAsync();
