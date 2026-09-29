using System.Net;
using System.Text.Json;
using Dawaya.Tests.Infrastructure;

namespace Dawaya.Tests.Api;

public sealed class HealthEndpointTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Health_reports_healthy_database_with_string_enums_and_utc_time()
    {
        using var client = factory.CreateClient();

        using var response = await client.GetAsync(new Uri("/api/v1/health", UriKind.Relative));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = json.RootElement;
        Assert.Equal("Healthy", root.GetProperty("status").GetString());
        Assert.Equal("Healthy", root.GetProperty("checks").GetProperty("database").GetString());
        // CLAUDE.md rule 8: dates are UTC ISO 8601
        Assert.EndsWith("Z", root.GetProperty("checkedAt").GetString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task OpenApi_document_describes_the_health_endpoint()
    {
        using var client = factory.CreateClient();

        var doc = await client.GetStringAsync(new Uri("/openapi/v1.json", UriKind.Relative));

        Assert.Contains("/api/v1/health", doc, StringComparison.Ordinal);
        Assert.Contains("\"Dawaya API\"", doc, StringComparison.Ordinal);
    }
}
