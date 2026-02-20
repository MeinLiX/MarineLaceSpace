using MarineLaceSpace.Models.Events;

namespace MarineLaceSpace.Interfaces.EventBus;

public interface IEventBus
{
    Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : IntegrationEvent;
    void Subscribe<T>(Func<T, CancellationToken, Task> handler) where T : IntegrationEvent;
}
