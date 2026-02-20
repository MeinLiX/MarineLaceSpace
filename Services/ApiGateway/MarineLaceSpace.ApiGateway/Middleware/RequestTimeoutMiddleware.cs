namespace ApiGateway.WebHost.Middleware;

public class RequestTimeoutMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestTimeoutMiddleware> _logger;
    private readonly TimeSpan _timeout;

    public RequestTimeoutMiddleware(RequestDelegate next, ILogger<RequestTimeoutMiddleware> logger, TimeSpan? timeout = null)
    {
        _next = next;
        _logger = logger;
        _timeout = timeout ?? TimeSpan.FromSeconds(30);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(context.RequestAborted);
        cts.CancelAfter(_timeout);

        context.RequestAborted = cts.Token;

        try
        {
            await _next(context);
        }
        catch (OperationCanceledException) when (cts.IsCancellationRequested && !context.RequestAborted.IsCancellationRequested)
        {
            _logger.LogWarning("Request {Method} {Path} timed out after {Timeout}s",
                context.Request.Method, context.Request.Path, _timeout.TotalSeconds);

            context.Response.StatusCode = StatusCodes.Status504GatewayTimeout;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Request timed out.",
                correlationId = context.Items["CorrelationId"]?.ToString()
            });
        }
    }
}
