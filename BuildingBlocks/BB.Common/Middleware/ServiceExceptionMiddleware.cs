using MarineLaceSpace.DTO.Responses;
using MarineLaceSpace.Exceptions.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace BB.Common.Middleware;

public class ServiceExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ServiceExceptionMiddleware> _logger;

    public ServiceExceptionMiddleware(RequestDelegate next, ILogger<ServiceExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (NotFoundEntityException ex)
        {
            _logger.LogWarning(ex, "Entity not found: {Message}", ex.Message);
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(RESTResult.Fail(ex.Message ?? "Entity not found."));
        }
        catch (ValidationEntityException ex)
        {
            _logger.LogWarning(ex, "Validation error: {Message}", ex.Message);
            context.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(RESTResult.Fail(ex.Message ?? "Validation error."));
        }
        catch (UserManagerException ex)
        {
            _logger.LogWarning(ex, "User manager error: {Message}", ex.Message);
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(RESTResult.Fail(ex.Message ?? "User operation failed."));
        }
        catch (Exception ex)
        {
            var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() ?? "unknown";
            _logger.LogError(ex, "Unhandled service exception. CorrelationId: {CorrelationId}", correlationId);
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { error = "An internal error occurred.", correlationId });
        }
    }
}
