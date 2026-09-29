using System.Net;
using Dawaya.Api.Features.Auth;
using Dawaya.Tests.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Dawaya.Tests.Api;

public sealed class SecurityTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    // Every endpoint requires sign-in unless it is on this list. Adding an anonymous endpoint is a
    // deliberate decision that must be made here (cross-cutting checklist: authorization on every endpoint).
    private static readonly string[] AnonymousAllowList =
    [
        "api/v1/_test/throw",
        "api/v1/_test/validate",
        "api/v1/auth/forgot-password",
        "api/v1/auth/login",
        "api/v1/auth/logout",
        "api/v1/auth/refresh",
        "api/v1/auth/register",
        "api/v1/auth/reset-password",
        "api/v1/health",
        "api/v1/ingredients",
        "openapi/{documentName}.json",
    ];

    [Fact]
    public void Only_allow_listed_endpoints_are_anonymous()
    {
        var endpoints = factory.Services.GetRequiredService<EndpointDataSource>().Endpoints.OfType<RouteEndpoint>();

        var anonymous = endpoints
            .Where(e => e.Metadata.GetMetadata<IAllowAnonymous>() is not null)
            .Select(e => e.RoutePattern.RawText!.TrimStart('/'))
            .Distinct()
            .Order(StringComparer.Ordinal);

        Assert.Equal(AnonymousAllowList.Order(StringComparer.Ordinal), anonymous);
    }

    [Fact]
    public async Task Auth_endpoints_are_rate_limited_with_a_problem_response()
    {
        using var limited = new RateLimitedApiFactory();
        using var client = limited.CreateClient();
        var body = new LoginRequest("someone@example.test", "whatever1");

        for (var i = 0; i < RateLimitedApiFactory.Limit; i++)
            (await client.PostJsonAsync("/api/v1/auth/login", body)).Dispose();
        using var blocked = await client.PostJsonAsync("/api/v1/auth/login", body);

        await blocked.AssertProblemAsync(HttpStatusCode.TooManyRequests, "rate_limited");
    }

    private sealed class RateLimitedApiFactory : ApiFactory
    {
        public const int Limit = 3;
        protected override int AuthPermitLimit => Limit;
    }
}
