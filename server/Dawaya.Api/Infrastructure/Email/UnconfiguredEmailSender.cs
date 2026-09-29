namespace Dawaya.Api.Infrastructure.Email;

// Used until a production provider is chosen (Slice 9). Logs that an email was dropped, never its content.
public sealed partial class UnconfiguredEmailSender(ILogger<UnconfiguredEmailSender> logger) : IEmailSender
{
    public Task SendAsync(EmailMessage message, CancellationToken ct)
    {
        LogDropped(logger);
        return Task.CompletedTask;
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "No email provider is configured; an email was not sent.")]
    private static partial void LogDropped(ILogger logger);
}
