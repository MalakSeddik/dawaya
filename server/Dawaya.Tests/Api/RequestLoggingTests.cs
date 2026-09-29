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

        // The request line is logged after the response is sent, so give it a moment to arrive.
        var logged = await factory.Logs.WaitForAsync(m => m.Contains("GET api/v1/health responded 200", StringComparison.Ordinal));
        Assert.True(logged, "Expected a request log line with the route template.");
        var messages = factory.Logs.Messages;
        Assert.DoesNotContain(messages, m => m.Contains("raw-share-token-123", StringComparison.Ordinal));
        Assert.DoesNotContain(messages, m => m.Contains("penicillin", StringComparison.Ordinal));
    }
}
