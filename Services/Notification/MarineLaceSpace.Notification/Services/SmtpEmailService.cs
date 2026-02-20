namespace Notification.WebHost.Services;

/// <summary>
/// Simulated email service that logs emails. 
/// Replace with actual SMTP/SendGrid implementation in production.
/// </summary>
public class SmtpEmailService : IEmailService
{
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(ILogger<SmtpEmailService> logger)
    {
        _logger = logger;
    }

    public Task SendEmailAsync(string to, string subject, string body, bool isHtml = false)
    {
        _logger.LogInformation(
            "[EMAIL] To: {To}, Subject: {Subject}, IsHtml: {IsHtml}, Body: {Body}",
            to, subject, isHtml, body.Length > 200 ? body[..200] + "..." : body);

        // TODO: Implement actual SMTP/SendGrid email sending
        // var message = new MimeMessage();
        // message.From.Add(new MailboxAddress("MarineLaceSpace", "noreply@marinelacespace.com"));
        // ...

        return Task.CompletedTask;
    }

    public Task SendTemplatedEmailAsync(string to, string templateName, Dictionary<string, string> parameters)
    {
        var paramStr = string.Join(", ", parameters.Select(p => $"{p.Key}={p.Value}"));
        _logger.LogInformation(
            "[EMAIL-TEMPLATE] To: {To}, Template: {Template}, Params: {Params}",
            to, templateName, paramStr);

        return Task.CompletedTask;
    }
}
