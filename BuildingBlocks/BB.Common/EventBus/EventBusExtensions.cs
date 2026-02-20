using MarineLaceSpace.Interfaces.EventBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BB.Common.EventBus;

public static class EventBusExtensions
{
    public static IServiceCollection AddRabbitMQEventBus(this IServiceCollection services, string connectionString, string serviceName, Action<RabbitMQEventBus>? configureSubscriptions = null)
    {
        services.AddSingleton<RabbitMQEventBus>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<RabbitMQEventBus>>();
            var bus = RabbitMQEventBus.CreateAsync(connectionString, logger).GetAwaiter().GetResult();
            configureSubscriptions?.Invoke(bus);
            return bus;
        });

        services.AddSingleton<IEventBus>(sp => sp.GetRequiredService<RabbitMQEventBus>());

        services.AddHostedService(sp =>
        {
            var bus = sp.GetRequiredService<RabbitMQEventBus>();
            var logger = sp.GetRequiredService<ILogger<EventBusBackgroundService>>();
            return new EventBusBackgroundService(bus, serviceName, logger);
        });

        return services;
    }
}
