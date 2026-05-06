using System.Threading.RateLimiting;
using ApiGateway.WebHost.Middleware;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
                builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                ?? ["http://localhost:3000", "http://localhost:5173", "https://localhost:5173"])
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProvider>();
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
    options.MimeTypes = Microsoft.AspNetCore.ResponseCompression.ResponseCompressionDefaults.MimeTypes.Concat(
        ["application/json", "text/plain"]);
});

builder.Services.Configure<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.Fastest;
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddFixedWindowLimiter("fixed", opt =>
    {
        opt.PermitLimit = 100;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 10;
    });

    options.AddSlidingWindowLimiter("sliding", opt =>
    {
        opt.PermitLimit = 60;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.SegmentsPerWindow = 6;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 5;
    });

    options.AddTokenBucketLimiter("auth", opt =>
    {
        opt.TokenLimit = 20;
        opt.ReplenishmentPeriod = TimeSpan.FromMinutes(1);
        opt.TokensPerPeriod = 10;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 5;
    });

    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(clientIp, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 200,
            Window = TimeSpan.FromMinutes(1)
        });
    });
});

builder.AddUseAfterBuild(
    app => app.UseCors(),
    app => app.UseResponseCompression(),
    app => app.UseRateLimiter(),
    app => app.UseWebSockets()
);

builder.AddServiceDefaults();

var app = builder.BuildWithPostActions();

app.UseMiddleware<CorrelationMiddleware>();
app.UseMiddleware<ExceptionHandlerMiddleware>();
app.UseMiddleware<RequestTimeoutMiddleware>();
app.UseMiddleware<RequestLoggerMiddleware>();

var authClient = "auth-api";
var catalogClient = "catalog-api";
var basketClient = "basket-api";
var orderClient = "order-api";
var paymentClient = "payment-api";
var notificationClient = "notification-api";
var minioClient = "minio";

app.MapForward("/minio/{**rest}", minioClient, "");

app.MapForward("/api/auth/{**rest}", authClient, "/auth").RequireRateLimiting("auth");
app.MapForward("/api/users/{**rest}", authClient, "/users");

app.MapForward("/api/shops/{shopId}/products/{**rest}", catalogClient, "/api/v1/shops/{shopId}/products");
app.MapForward("/api/shops/{**rest}", catalogClient, "/shops");
app.MapForward("/api/categories/{**rest}", catalogClient, "/api/categories");

app.MapForward("/api/product-images/{**rest}", catalogClient, "/api/products");
app.MapForward("/api/photo/{**rest}", catalogClient, "/api/images");

app.MapForward("/api/products/{**rest}", catalogClient, "/api/v1/products");

app.MapForward("/api/reviews/{**rest}", catalogClient, "/api/reviews");

app.MapForward("/api/sizes/{**rest}", catalogClient, "/api/sizes");
app.MapForward("/api/colors/{**rest}", catalogClient, "/api/colors");
app.MapForward("/api/materials/{**rest}", catalogClient, "/api/materials");

app.MapForward("/api/basket/{**rest}", basketClient, "/api/basket");

app.MapForward("/api/orders/{**rest}", orderClient, "/api/orders");

app.MapForward("/api/payments/{**rest}", paymentClient, "/api/payments");

app.MapForward("/api/notifications/hub/negotiate", notificationClient, "/api/notifications/hub/negotiate");
app.MapForward("/api/notifications/hub", notificationClient, "/api/notifications/hub");
app.MapForward("/api/notifications/{**rest}", notificationClient, "/api/notifications");

app.MapGet("/api/health", async (IHttpClientFactory httpClientFactory) =>
{
    var services = new[] { authClient, catalogClient, basketClient, orderClient, paymentClient, notificationClient };
    var results = new Dictionary<string, object>();
    var overallHealthy = true;

    var tasks = services.Select(async service =>
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var client = httpClientFactory.CreateClient(service);
            var response = await client.GetStringAsync("/health");
            sw.Stop();
            return (service, result: (object)new { status = "Healthy", responseTimeMs = sw.ElapsedMilliseconds, details = response });
        }
        catch (Exception ex)
        {
            sw.Stop();
            return (service, result: (object)new { status = "Unhealthy", responseTimeMs = sw.ElapsedMilliseconds, error = ex.Message });
        }
    });

    var completedTasks = await Task.WhenAll(tasks);
    foreach (var (service, result) in completedTasks)
    {
        results[service] = result;
        if (result.GetType().GetProperty("status")?.GetValue(result)?.ToString() != "Healthy")
            overallHealthy = false;
    }

    var healthResponse = new
    {
        status = overallHealthy ? "Healthy" : "Degraded",
        timestamp = DateTime.UtcNow,
        services = results
    };

    return overallHealthy ? Results.Ok(healthResponse) : Results.Json(healthResponse, statusCode: StatusCodes.Status207MultiStatus);
}).WithTags("Health").WithSummary("Aggregated health check with timing");

await app.RunAsync();

public static class GatewayExtensions
{
    public static RouteHandlerBuilder MapForward(this WebApplication app, string pattern, string serviceName, string targetPrefix)
    {
        return app.Map(pattern, async (HttpContext context, IHttpClientFactory httpClientFactory) =>
        {
            var client = httpClientFactory.CreateClient(serviceName);
            var rest = context.Request.RouteValues["rest"]?.ToString() ?? "";

        var resolvedPrefix = targetPrefix;
        foreach (var rv in context.Request.RouteValues)
        {
            if (rv.Key != "rest" && rv.Value != null)
                resolvedPrefix = resolvedPrefix.Replace($"{{{rv.Key}}}", rv.Value.ToString());
        }

        var targetPath = string.IsNullOrEmpty(rest) ? resolvedPrefix : $"{resolvedPrefix}/{rest}";
                targetPath += context.Request.QueryString.Value;

            var requestMessage = new HttpRequestMessage
            {
                Method = new HttpMethod(context.Request.Method),
                RequestUri = new Uri(targetPath, UriKind.Relative)
            };

            foreach (var header in context.Request.Headers)
            {
                if (!header.Key.StartsWith("Host", StringComparison.OrdinalIgnoreCase))
                    requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
            }

            var correlationId = context.Items["CorrelationId"]?.ToString();
            if (!string.IsNullOrEmpty(correlationId))
            {
                requestMessage.Headers.TryAddWithoutValidation("X-Correlation-ID", correlationId);
            }

            // Buffer body into memory so Polly retries can re-read it
            if (context.Request.ContentLength > 0 || context.Request.ContentType != null)
            {
                var ms = new MemoryStream();
                await context.Request.Body.CopyToAsync(ms);
                ms.Position = 0;
                requestMessage.Content = new StreamContent(ms);
                if (context.Request.ContentType != null)
                    requestMessage.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse(context.Request.ContentType);
                if (context.Request.ContentLength.HasValue)
                    requestMessage.Content.Headers.ContentLength = context.Request.ContentLength.Value;
            }

            var response = await client.SendAsync(requestMessage);

            context.Response.StatusCode = (int)response.StatusCode;
            foreach (var header in response.Headers)
                context.Response.Headers[header.Key] = header.Value.ToArray();
            foreach (var header in response.Content.Headers)
                context.Response.Headers[header.Key] = header.Value.ToArray();

            context.Response.Headers.Remove("transfer-encoding");

            var statusCode = (int)response.StatusCode;
            if (statusCode != 204 && statusCode != 304)
            {
                await response.Content.CopyToAsync(context.Response.Body);
            }
        });
    }

    public static void MapWebSocketForward(this WebApplication app, string pattern, string httpClientName, string targetPath)
    {
        app.Map(pattern, async (HttpContext context) =>
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("WebSocket upgrade required");
                return;
            }

            var factory = context.RequestServices.GetRequiredService<IHttpClientFactory>();
            var client = factory.CreateClient(httpClientName);

            // Redirect the WebSocket client to the actual notification service
            context.Response.StatusCode = StatusCodes.Status307TemporaryRedirect;
            context.Response.Headers.Location = $"{client.BaseAddress}{targetPath}";
        });
    }
}
