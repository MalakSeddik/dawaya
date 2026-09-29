using System.Net;
using Dawaya.Api.Features.Profiles;
using Dawaya.Tests.Infrastructure;

namespace Dawaya.Tests.Api;

// Roadmap Slice 1: a user can never read or change another user's profile. Other people's profiles
// answer "not found" (not "forbidden"), so their ids cannot even be confirmed to exist.
public sealed class OwnershipTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task A_user_cannot_touch_another_users_profiles_or_allergies()
    {
        var (alice, _) = await factory.CreateSignedInClientAsync();
        var (bob, _) = await factory.CreateSignedInClientAsync();
        using var _a = alice;
        using var _b = bob;

        var aliceMom = await (await alice.PostJsonAsync("/api/v1/profiles",
            new SaveProfileRequest("Alice's mom", null, null, false, false))).ReadAsync<ProfileResponse>();
        var withAllergy = await (await alice.PostJsonAsync($"/api/v1/profiles/{aliceMom.Id}/allergies",
            new AddAllergyRequest(null, "Sulfa"))).ReadAsync<ProfileResponse>();
        var allergyId = withAllergy.Allergies.Single().Id;
        var url = $"/api/v1/profiles/{aliceMom.Id}";

        using (var get = await bob.GetAsync(url)) await get.AssertProblemAsync(HttpStatusCode.NotFound, "profile_not_found");
        using (var put = await bob.PutJsonAsync(url, new SaveProfileRequest("Hacked", null, null, false, false)))
            await put.AssertProblemAsync(HttpStatusCode.NotFound, "profile_not_found");
        using (var del = await bob.DeleteAsync(url)) await del.AssertProblemAsync(HttpStatusCode.NotFound, "profile_not_found");
        using (var add = await bob.PostJsonAsync($"{url}/allergies", new AddAllergyRequest(null, "Injected")))
            await add.AssertProblemAsync(HttpStatusCode.NotFound, "profile_not_found");
        using (var remove = await bob.DeleteAsync($"{url}/allergies/{allergyId}"))
            await remove.AssertProblemAsync(HttpStatusCode.NotFound, "allergy_not_found");

        var bobsList = await (await bob.GetAsync("/api/v1/profiles")).ReadAsync<List<ProfileResponse>>();
        Assert.DoesNotContain(bobsList, p => p.Id == aliceMom.Id);

        // Alice's data is untouched.
        var after = await (await alice.GetAsync(url)).ReadAsync<ProfileResponse>();
        Assert.Equal("Alice's mom", after.DisplayName);
        Assert.Equal("Sulfa", Assert.Single(after.Allergies).AllergyText);
    }

    [Fact]
    public async Task An_allergy_id_from_one_profile_cannot_be_removed_through_another_profile()
    {
        var (client, _) = await factory.CreateSignedInClientAsync();
        using var _c = client;
        var mom = await (await client.PostJsonAsync("/api/v1/profiles", new SaveProfileRequest("Mom", null, null, false, false))).ReadAsync<ProfileResponse>();
        var dad = await (await client.PostJsonAsync("/api/v1/profiles", new SaveProfileRequest("Dad", null, null, false, false))).ReadAsync<ProfileResponse>();
        var momAllergy = (await (await client.PostJsonAsync($"/api/v1/profiles/{mom.Id}/allergies", new AddAllergyRequest(null, "Latex"))).ReadAsync<ProfileResponse>()).Allergies.Single();

        using var response = await client.DeleteAsync($"/api/v1/profiles/{dad.Id}/allergies/{momAllergy.Id}");

        await response.AssertProblemAsync(HttpStatusCode.NotFound, "allergy_not_found");
    }

    [Theory]
    [InlineData("GET", "/api/v1/profiles")]
    [InlineData("POST", "/api/v1/profiles")]
    [InlineData("GET", "/api/v1/profiles/1")]
    [InlineData("GET", "/api/v1/me")]
    [InlineData("GET", "/api/v1/me/export")]
    [InlineData("POST", "/api/v1/me/delete")]
    public async Task Signed_out_requests_get_401_problem_details(string method, string url)
    {
        using var client = factory.CreateClient();
        using var request = new HttpRequestMessage(new HttpMethod(method), new Uri(url, UriKind.Relative));
        if (method == "POST") request.Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");

        using var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task A_tampered_token_is_rejected()
    {
        var (client, auth) = await factory.CreateSignedInClientAsync();
        using var _c = client;
        var parts = auth.AccessToken.Split('.');
        client.DefaultRequestHeaders.Authorization = new("Bearer", $"{parts[0]}.{parts[1]}.{new string('A', parts[2].Length)}");

        using var response = await client.GetAsync("/api/v1/profiles");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
