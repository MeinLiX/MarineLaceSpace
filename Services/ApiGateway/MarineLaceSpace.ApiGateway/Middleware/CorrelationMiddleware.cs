namespace ApiGateway.WebHost.Middleware;

public class CorrelationMiddleware
{
    private readonly RequestDelegate _next;
    private const string CorrelationHeaderName = "X-Correlation-ID";

    public CorrelationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[CorrelationHeaderName].FirstOrDefault()
            ?? Guid.NewGuid().ToString();

        context.Items["CorrelationId"] = correlationId;

        context.Response.Headers[CorrelationHeaderName] = correlationId;

        await _next(context);
    }
}