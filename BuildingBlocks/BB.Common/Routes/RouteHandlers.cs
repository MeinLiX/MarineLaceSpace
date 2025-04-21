using FluentValidation;
using MarineLaceSpace.DTO.Responses;
using MarineLaceSpace.Models.Routes;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace BB.Common.Routes;

public class RouteHandlers
{
    public static async Task<IResult> RouteHandlerAsync<TDto, TRouteServices>(TDto dto,
                                                                          IServiceProvider serviceProvider,
                                                                          Func<TRouteServices, Task<IResult>> act)
    where TRouteServices : BasicRouteServices
    where TDto : class
    {
        var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
        var requestAborted = httpContextAccessor.HttpContext?.RequestAborted ?? CancellationToken.None;

        var validatorType = typeof(IValidator<>).MakeGenericType(typeof(TDto));
        if (serviceProvider.GetService(validatorType) is IValidator validator)
        {
            var validationContext = new ValidationContext<object>(dto);
            var validationResult = await validator.ValidateAsync(validationContext);

            if (!validationResult.IsValid)
            {
                return Results.Json(RESTErrorResult<IDictionary<string, string[]>>.Fail(validationResult.ToDictionary()), statusCode: 422);
            }
        }

        var routeServicesType = typeof(TRouteServices);
        var properties = routeServicesType.GetProperties()
            .Where(p => p.CanWrite && p.SetMethod != null);

        var routeServicesInstance = Activator.CreateInstance(routeServicesType) ?? throw new InvalidOperationException($"Could not create an instance of {routeServicesType.Name}");
        var routeServices = (TRouteServices)routeServicesInstance;

        var requestAbortedProperty = routeServicesType.GetProperty(nameof(BasicRouteServices.RequestAborted));
        requestAbortedProperty?.SetValue(routeServices, requestAborted);

        foreach (var property in properties)
        {
            if (property.Name == nameof(BasicRouteServices.RequestAborted))
                continue;

            var propertyType = property.PropertyType;
            var service = serviceProvider.GetService(propertyType);

            if (service != null)
            {
                property.SetValue(routeServices, service);
            }
            else if (property.GetCustomAttribute<RequiredAttribute>() != null)
            {
                throw new InvalidOperationException($"Required service of type {propertyType.Name} was not found in the service provider");
            }
        }

        return await act.Invoke(routeServices);
    }
}
