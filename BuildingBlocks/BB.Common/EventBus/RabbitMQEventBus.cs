using MarineLaceSpace.Interfaces.EventBus;
using MarineLaceSpace.Models.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;

namespace BB.Common.EventBus;

public class RabbitMQEventBus : IEventBus, IAsyncDisposable
{
    private readonly IConnection _connection;
    private readonly IChannel _channel;
    private readonly string _exchangeName = "marinelacespace_events";
    private readonly ILogger<RabbitMQEventBus> _logger;
    private readonly ConcurrentDictionary<string, List<Delegate>> _handlers = new();

    private RabbitMQEventBus(IConnection connection, IChannel channel, ILogger<RabbitMQEventBus> logger)
    {
        _connection = connection;
        _channel = channel;
        _logger = logger;
    }

    public static async Task<RabbitMQEventBus> CreateAsync(string connectionString, ILogger<RabbitMQEventBus> logger)
    {
        var factory = new ConnectionFactory { Uri = new Uri(connectionString) };
        var connection = await factory.CreateConnectionAsync();
        var channel = await connection.CreateChannelAsync();
        await channel.ExchangeDeclareAsync(exchange: "marinelacespace_events", type: ExchangeType.Topic, durable: true);
        return new RabbitMQEventBus(connection, channel, logger);
    }

    public async Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : IntegrationEvent
    {
        var eventName = typeof(T).Name;
        var message = JsonSerializer.Serialize(@event);
        var body = Encoding.UTF8.GetBytes(message);

        var properties = new BasicProperties
        {
            DeliveryMode = DeliveryModes.Persistent,
            MessageId = @event.Id.ToString(),
            Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
            ContentType = "application/json",
            Type = eventName
        };

        await _channel.BasicPublishAsync(
            exchange: _exchangeName,
            routingKey: eventName,
            mandatory: false,
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken);

        _logger.LogInformation("Published event {EventName} with ID {EventId}", eventName, @event.Id);
    }

    public void Subscribe<T>(Func<T, CancellationToken, Task> handler) where T : IntegrationEvent
    {
        var eventName = typeof(T).Name;
        _handlers.AddOrUpdate(eventName,
            _ => new List<Delegate> { handler },
            (_, list) => { list.Add(handler); return list; });
    }

    public async Task StartConsumingAsync(string serviceName, CancellationToken cancellationToken = default)
    {
        foreach (var eventName in _handlers.Keys)
        {
            var queueName = $"{serviceName}_{eventName}";
            await _channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false, cancellationToken: cancellationToken);
            await _channel.QueueBindAsync(queue: queueName, exchange: _exchangeName, routingKey: eventName, cancellationToken: cancellationToken);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (_, ea) =>
            {
                try
                {
                    var body = Encoding.UTF8.GetString(ea.Body.ToArray());
                    if (_handlers.TryGetValue(eventName, out var handlers))
                    {
                        foreach (var h in handlers)
                        {
                            var eventType = GetEventType(eventName);
                            if (eventType != null)
                            {
                                var @event = JsonSerializer.Deserialize(body, eventType);
                                if (@event != null)
                                {
                                    await (Task)h.DynamicInvoke(@event, cancellationToken)!;
                                }
                            }
                        }
                    }
                    await _channel.BasicAckAsync(ea.DeliveryTag, false, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing event {EventName}", eventName);
                    await _channel.BasicNackAsync(ea.DeliveryTag, false, true, cancellationToken);
                }
            };

            await _channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer, cancellationToken: cancellationToken);
            _logger.LogInformation("Started consuming {EventName} on queue {QueueName}", eventName, queueName);
        }
    }

    private static Type? GetEventType(string eventName)
    {
        var assembly = typeof(IntegrationEvent).Assembly;
        return assembly.GetTypes().FirstOrDefault(t => t.Name == eventName && t.IsSubclassOf(typeof(IntegrationEvent)));
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel.IsOpen)
            await _channel.CloseAsync();
        if (_connection.IsOpen)
            await _connection.CloseAsync();
    }
}
