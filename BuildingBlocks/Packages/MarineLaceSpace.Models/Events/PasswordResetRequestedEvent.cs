namespace MarineLaceSpace.Models.Events;

public class PasswordResetRequestedEvent : IntegrationEvent
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string ResetToken { get; set; } = string.Empty;
}
