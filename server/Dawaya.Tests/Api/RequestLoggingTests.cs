using Dawaya.Tests.Infrastructure;

namespace Dawaya.Tests.Api;

// BR-36: tokens and health data never reach the logs. Request logs carry the route template only.
public sealed class RequestLoggingTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Request_log_uses_route_template_and_never_the_query_string()
    {
        using var client = factory.CreateClient();

        using var response = await client.GetAsync(
            new Uri("/api/v1/health?token=raw-share-token-123&allergy=penicillin", UriKind.Relative));

        var messages = factory.Logs.Messages;
        Assert.Contains(messages, m => m.Contains("GET api/v1/health responded 200", StringComparison.Ordinal));
        Assert.DoesNotContain(messages, m => m.Contains("raw-share-token-123", StringComparison.Ordinal));
        Assert.DoesNotContain(messages, m => m.Contains("penicillin", StringComparison.Ordinal));
    }
}
