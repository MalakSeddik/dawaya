using System.Globalization;
using System.Text;

namespace Dawaya.Api.Infrastructure.Email;

// Development only: writes each email to a file in a git-ignored folder. Emails can contain reset
// tokens, so their content never goes to the logs.
public sealed class FileEmailSender(string directory, TimeProvider clock) : IEmailSender
{
    public async Task SendAsync(EmailMessage message, CancellationToken ct)
    {
        Directory.CreateDirectory(directory);
        var name = $"{clock.GetUtcNow():yyyyMMdd-HHmmss-fff}-{Guid.NewGuid():N}.txt";
        var content = new StringBuilder()
            .AppendLine(CultureInfo.InvariantCulture, $"To: {message.To}")
            .AppendLine(CultureInfo.InvariantCulture, $"Subject: {message.Subject}")
            .AppendLine()
            .AppendLine(message.TextBody)
            .ToString();
        await File.WriteAllTextAsync(Path.Combine(directory, name), content, Encoding.UTF8, ct);
    }
}
