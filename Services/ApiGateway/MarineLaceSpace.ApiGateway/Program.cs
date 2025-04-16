using ApiGateway.WebHost.Aggregators;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddApiAggregators();

var app = builder.Build();

app.MapGet("/", async (IHttpClientFactory httpClientFactory) =>
{
    var orderClient = httpClientFactory.CreateClient("order-api");
    var basketClient = httpClientFactory.CreateClient("basket-api");

    var orderRes = orderClient.GetStringAsync("/");
    var basketRes = basketClient.GetStringAsync("/");

    await Task.WhenAll(orderRes, basketRes);
    
    return $"order: {orderRes.Result}; basket: {basketRes.Result}";
});

app.Run();
