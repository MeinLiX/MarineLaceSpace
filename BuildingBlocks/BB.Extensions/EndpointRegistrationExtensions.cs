/** An interesting approach endpoint declaration - but it's quite complex and its behavior is unpredictable without thorough testing.

namespace BB.Extensions;

public static class EndpointRegistrationExtensions
{

    
    public static void MapEndpointsFromInterface<TInterface>(this WebApplication app)
        where TInterface : class
    {
        var interfaceType = typeof(TInterface);

        var methods = interfaceType.GetMethods()
            .Where(m => m.GetCustomAttribute<RoutePatternAttribute>() != null);

        foreach (var method in methods)
        {
            var pattern = method.GetCustomAttribute<RoutePatternAttribute>();
            RegisterEndpoint(app, interfaceType, method, pattern);
        }
    }

    private static void RegisterEndpoint(
        WebApplication app,
        Type interfaceType,
        MethodInfo method,
        RoutePatternAttribute pattern)
    {
        var route = pattern.Route;
        var handlerType = typeof(EndpointHandler<>).MakeGenericType(interfaceType);
        var handler = Activator.CreateInstance(handlerType, method);

        switch (pattern.Method.Method)
        {
            case MarineLaceSpace.Enumerations.HttpMethods.Get:
                app.MapGet(route, (HttpContext context, [FromServices] IServiceProvider services) =>
                    ((EndpointHandler)handler).HandleAsync(context, services));
                break;
            case MarineLaceSpace.Enumerations.HttpMethods.Post:
                app.MapPost(route, (HttpContext context, [FromServices] IServiceProvider services) =>
                    ((EndpointHandler)handler).HandleAsync(context, services));
                break;
            case MarineLaceSpace.Enumerations.HttpMethods.Put:
                app.MapPut(route, (HttpContext context, [FromServices] IServiceProvider services) =>
                    ((EndpointHandler)handler).HandleAsync(context, services));
                break;
            case MarineLaceSpace.Enumerations.HttpMethods.Delete:
                app.MapDelete(route, (HttpContext context, [FromServices] IServiceProvider services) =>
                    ((EndpointHandler)handler).HandleAsync(context, services));
                break;
        }
    }

    private abstract class EndpointHandler
    {
        public abstract Task HandleAsync(HttpContext context, IServiceProvider services);
    }

    private class EndpointHandler<TInterface> : EndpointHandler where TInterface : class
    {
        private readonly MethodInfo _method;

        public EndpointHandler(MethodInfo method)
        {
            _method = method;
        }

        public override async Task HandleAsync(HttpContext context, IServiceProvider services)
        {
            var implementation = services.GetRequiredService<TInterface>();

            var parameters = _method.GetParameters();
            var paramValues = new object[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                var param = parameters[i];

                if (context.Request.RouteValues.TryGetValue(param.Name, out var routeValue))
                {
                    paramValues[i] = ConvertValue(routeValue, param.ParameterType);
                    continue;
                }

                var queryValue = context.Request.Query[param.Name];
                if (queryValue.Count > 0)
                {
                    paramValues[i] = ConvertValue(queryValue.ToString(), param.ParameterType);
                    continue;
                }

                if (context.Request.HasJsonContentType() && !param.ParameterType.IsPrimitive
                    && param.ParameterType != typeof(string))
                {
                    context.Request.EnableBuffering();

                    using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
                    var requestBody = await reader.ReadToEndAsync();

                    context.Request.Body.Position = 0;

                    if (!string.IsNullOrEmpty(requestBody))
                    {
                        paramValues[i] = JsonSerializer.Deserialize(
                            requestBody, param.ParameterType,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    }
                    continue;
                }

                if (param.ParameterType == typeof(HttpContext))
                {
                    paramValues[i] = context;
                    continue;
                }

                if (!param.ParameterType.IsPrimitive && param.ParameterType != typeof(string))
                {
                    try
                    {
                        paramValues[i] = services.GetService(param.ParameterType);
                        continue;
                    }
                    catch
                    {
                    }
                }

                paramValues[i] = param.HasDefaultValue ? param.DefaultValue : null;
            }

            var result = _method.Invoke(implementation, paramValues);

            if (result is Task task)
            {
                await task;

                if (task.GetType().IsGenericType)
                {
                    var taskResultProperty = task.GetType().GetProperty("Result");
                    result = taskResultProperty.GetValue(task);
                }
                else
                {
                    result = null;
                }
            }

            if (result is IResult actionResult)
            {
                await actionResult.ExecuteAsync(context);
            }
            else if (result != null)
            {
                await context.Response.WriteAsJsonAsync(result);
            }
        }

        private static object? ConvertValue(object value, Type targetType)
        {
            if (value == null) return null;

            try
            {
                if (targetType == typeof(Guid) && value is string str)
                {
                    return Guid.Parse(str);
                }

                return Convert.ChangeType(value, targetType);
            }
            catch
            {
                return null;
            }
        }
    }
}
*/