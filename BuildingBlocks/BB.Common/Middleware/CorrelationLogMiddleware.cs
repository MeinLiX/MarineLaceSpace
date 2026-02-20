using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace BB.Common.Middleware;

public class CorrelationLogMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationLogMiddleware> _logger;

    public CorrelationLogMiddleware(RequestDelegate next, ILogger<CorrelationLogMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault()
                            ?? context.Items["CorrelationId"]?.ToString()
                            ?? Guid.NewGuid().ToString();

        context.Items["CorrelationId"] = correlationId;

        using (_logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId }))
        {
            await _next(context);
        }
    }
}
