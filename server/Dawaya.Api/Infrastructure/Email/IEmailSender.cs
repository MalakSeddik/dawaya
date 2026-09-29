namespace Dawaya.Api.Infrastructure.Email;

public sealed record EmailMessage(string To, string Subject, string TextBody);

// Provider is replaceable (CLAUDE.md rule 5/6). The production provider is chosen in Slice 9.
public interface IEmailSender
{
    Task SendAsync(EmailMessage message, CancellationToken ct);
}
