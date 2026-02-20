using ApiGateway.WebHost.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var app = builder.BuildWithPostActions();

// Middleware pipeline
app.UseMiddleware<CorrelationMiddleware>();
app.UseMiddleware<RequestLoggerMiddleware>();

// Reverse Proxy routes — forward to microservices
var authClient = "auth-api";
var catalogClient = "catalog-api";
var basketClient = "basket-api";
var orderClient = "order-api";
var paymentClient = "payment-api";
var notificationClient = "notification-api";

// Auth routes
app.MapForward("/api/auth/{**rest}", authClient, "/auth");
app.MapForward("/api/users/{**rest}", authClient, "/users");

// Catalog routes
app.MapForward("/api/shops/{**rest}", catalogClient, "/shops");
app.MapForward("/api/categories/{**rest}", catalogClient, "/api/categories");
app.MapForward("/api/products/{**rest}", catalogClient, "/api/v1/products");
app.MapForward("/api/sizes/{**rest}", catalogClient, "/api/sizes");
app.MapForward("/api/colors/{**rest}", catalogClient, "/api/colors");
app.MapForward("/api/materials/{**rest}", catalogClient, "/api/materials");

// Basket routes
app.MapForward("/api/basket/{**rest}", basketClient, "/api/basket");

// Order routes
app.MapForward("/api/orders/{**rest}", orderClient, "/api/orders");

// Payment routes
app.MapForward("/api/payments/{**rest}", paymentClient, "/api/payments");

// Notification routes
app.MapForward("/api/notifications/{**rest}", notificationClient, "/api/notifications");

// Health check aggregation
app.MapGet("/api/health", async (IHttpClientFactory httpClientFactory) =>
{
    var services = new[] { authClient, catalogClient, basketClient, orderClient, paymentClient, notificationClient };
    var results = new Dictionary<string, string>();

    foreach (var service in services)
    {
        try
        {
            var client = httpClientFactory.CreateClient(service);
            var response = await client.GetStringAsync("/health");
            results[service] = response;
        }
        catch (Exception ex)
        {
            results[service] = $"Error: {ex.Message}";
        }
    }

    return Results.Ok(results);
}).WithTags("Health").WithSummary("Aggregated health check");

await app.RunAsync();

public static class GatewayExtensions
{
    public static void MapForward(this WebApplication app, string pattern, string serviceName, string targetPrefix)
    {
        app.Map(pattern, async (HttpContext context, IHttpClientFactory httpClientFactory) =>
        {
            var client = httpClientFactory.CreateClient(serviceName);
            var rest = context.Request.RouteValues["rest"]?.ToString() ?? "";
            var targetPath = string.IsNullOrEmpty(rest) ? targetPrefix : $"{targetPrefix}/{rest}";

            if (context.Request.QueryString.HasValue)
                targetPath += context.Request.QueryString.Value;

            var requestMessage = new HttpRequestMessage
            {
                Method = new HttpMethod(context.Request.Method),
                RequestUri = new Uri(targetPath, UriKind.Relative)
            };

            // Forward headers
            foreach (var header in context.Request.Headers)
            {
                if (!header.Key.StartsWith("Host", StringComparison.OrdinalIgnoreCase))
                    requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
            }

            // Forward body for POST/PUT/PATCH
            if (context.Request.ContentLength > 0 || context.Request.ContentType != null)
            {
                requestMessage.Content = new StreamContent(context.Request.Body);
                if (context.Request.ContentType != null)
                    requestMessage.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(context.Request.ContentType);
            }

            var response = await client.SendAsync(requestMessage);

            context.Response.StatusCode = (int)response.StatusCode;
            foreach (var header in response.Headers)
                context.Response.Headers[header.Key] = header.Value.ToArray();
            foreach (var header in response.Content.Headers)
                context.Response.Headers[header.Key] = header.Value.ToArray();

            context.Response.Headers.Remove("transfer-encoding");

            await response.Content.CopyToAsync(context.Response.Body);
        });
    }
}
