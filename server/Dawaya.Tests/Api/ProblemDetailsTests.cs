using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Dawaya.Tests.Infrastructure;

namespace Dawaya.Tests.Api;

// CLAUDE.md rule 9: every error is RFC 7807 ProblemDetails.
public sealed class ProblemDetailsTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Unknown_route_returns_404_problem_details()
    {
        using var client = factory.CreateClient();

        using var response = await client.GetAsync(new Uri("/api/v1/does-not-exist", UriKind.Relative));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(404, json.RootElement.GetProperty("status").GetInt32());
        Assert.True(json.RootElement.TryGetProperty("traceId", out _));
    }

    [Fact]
    public async Task Unhandled_exception_returns_generic_500_without_leaking_or_logging_the_message()
    {
        using var client = factory.CreateClient();

        using var response = await client.GetAsync(new Uri("/api/v1/_test/throw", UriKind.Relative));

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var body = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain(TestOnlyController.SecretMessage, body, StringComparison.Ordinal);
        Assert.DoesNotContain("StackTrace", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(factory.Logs.Messages, m => m.Contains(TestOnlyController.SecretMessage, StringComparison.Ordinal));
    }

    [Fact]
    public async Task Invalid_input_returns_400_validation_problem_with_field_errors()
    {
        using var client = factory.CreateClient();

        using var response = await client.PostAsJsonAsync(
            new Uri("/api/v1/_test/validate", UriKind.Relative), new SampleRequest("", 0));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var errors = json.RootElement.GetProperty("errors");
        Assert.True(errors.TryGetProperty("DisplayName", out _));
        Assert.True(errors.TryGetProperty("Quantity", out _));
    }

    [Fact]
    public async Task Valid_input_passes_validation()
    {
        using var client = factory.CreateClient();

        using var response = await client.PostAsJsonAsync(
            new Uri("/api/v1/_test/validate", UriKind.Relative), new SampleRequest("Mom", 20));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
