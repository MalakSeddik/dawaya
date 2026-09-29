using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dawaya.Api.Features.Auth;

namespace Dawaya.Tests.Infrastructure;

public static class ApiClientExtensions
{
    public const string DefaultPassword = "correct horse battery";

    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    private static int _counter;

    public static string UniqueEmail(string name = "user") =>
        $"{name}{Interlocked.Increment(ref _counter)}-{Guid.NewGuid():N}@example.test";

    public static async Task<AuthResponse> RegisterAsync(this HttpClient client, string? email = null, string displayName = "Me",
        string password = DefaultPassword)
    {
        using var response = await client.PostAsJsonAsync(new Uri("/api/v1/auth/register", UriKind.Relative),
            new RegisterRequest(email ?? UniqueEmail(), password, displayName, "ar"), Json);
        await response.EnsureStatusAsync(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<AuthResponse>(Json))!;
    }

    // A client signed in as a brand-new user.
    public static async Task<(HttpClient Client, AuthResponse Auth)> CreateSignedInClientAsync(this ApiFactory factory,
        string? email = null)
    {
        var client = factory.CreateClient();
        var auth = await client.RegisterAsync(email);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        return (client, auth);
    }

    public static async Task<T> ReadAsync<T>(this HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<T>(Json))!;

    public static Task<HttpResponseMessage> PostJsonAsync<T>(this HttpClient client, string url, T body) =>
        client.PostAsJsonAsync(new Uri(url, UriKind.Relative), body, Json);

    public static Task<HttpResponseMessage> PutJsonAsync<T>(this HttpClient client, string url, T body) =>
        client.PutAsJsonAsync(new Uri(url, UriKind.Relative), body, Json);

    public static Task<HttpResponseMessage> GetAsync(this HttpClient client, string url) =>
        client.GetAsync(new Uri(url, UriKind.Relative));

    public static Task<HttpResponseMessage> DeleteAsync(this HttpClient client, string url) =>
        client.DeleteAsync(new Uri(url, UriKind.Relative));

    public static async Task EnsureStatusAsync(this HttpResponseMessage response, HttpStatusCode expected)
    {
        if (response.StatusCode != expected)
        {
            var body = await response.Content.ReadAsStringAsync();
            Assert.Fail($"Expected {(int)expected} but got {(int)response.StatusCode}: {body}");
        }
    }

    // Asserts an RFC 7807 problem with the given status and machine-readable code.
    public static async Task AssertProblemAsync(this HttpResponseMessage response, HttpStatusCode status, string code)
    {
        await response.EnsureStatusAsync(status);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(code, json.RootElement.GetProperty("code").GetString());
    }
}
