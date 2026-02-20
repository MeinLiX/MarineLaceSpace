using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BB.Common.EventBus;

public class EventBusBackgroundService : BackgroundService
{
    private readonly RabbitMQEventBus _eventBus;
    private readonly string _serviceName;
    private readonly ILogger<EventBusBackgroundService> _logger;

    public EventBusBackgroundService(RabbitMQEventBus eventBus, string serviceName, ILogger<EventBusBackgroundService> logger)
    {
        _eventBus = eventBus;
        _serviceName = serviceName;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("EventBus background service starting for {ServiceName}", _serviceName);
        await _eventBus.StartConsumingAsync(_serviceName, stoppingToken);
    }
}
