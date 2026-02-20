namespace Notification.WebHost.Services;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body, bool isHtml = false);
    Task SendTemplatedEmailAsync(string to, string templateName, Dictionary<string, string> parameters);
}
