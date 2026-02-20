namespace MarineLaceSpace.Models.Events;

public class UserRegisteredEvent : IntegrationEvent
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsAnonymous { get; set; }
}
