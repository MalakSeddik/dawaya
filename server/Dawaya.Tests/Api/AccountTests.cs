using System.Net;
using System.Text;
using Dawaya.Api.Domain;
using Dawaya.Api.Features.Account;
using Dawaya.Api.Features.Auth;
using Dawaya.Api.Features.Profiles;
using Dawaya.Api.Infrastructure.Storage;
using Dawaya.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Dawaya.Tests.Api;

public sealed class AccountTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Me_returns_the_account_and_preferences_follow_the_user()
    {
        var (client, auth) = await factory.CreateSignedInClientAsync();
        using var _c = client;

        var me = await (await client.GetAsync("/api/v1/me")).ReadAsync<CurrentUserResponse>();
        Assert.Equal(auth.User.Email, me.Email);
        Assert.False(me.LargeText);

        using var put = await client.PutJsonAsync("/api/v1/me/preferences", new UpdatePreferencesRequest("en", LargeText: true, HighContrast: true));
        await put.EnsureStatusAsync(HttpStatusCode.OK);

        // A fresh sign-in (e.g. another device) gets the saved preferences (BR-33).
        using var login = await client.PostJsonAsync("/api/v1/auth/login", new LoginRequest(auth.User.Email, ApiClientExtensions.DefaultPassword));
        var user = (await login.ReadAsync<AuthResponse>()).User;
        Assert.Equal("en", user.PreferredLanguage);
        Assert.True(user.LargeText);
        Assert.True(user.HighContrast);

        using var bad = await client.PutJsonAsync("/api/v1/me/preferences", new UpdatePreferencesRequest("fr", false, false));
        Assert.Equal(HttpStatusCode.BadRequest, bad.StatusCode);
    }

    // Roadmap "Done when": three profiles with allergies, export everything, delete so no rows remain.
    [Fact]
    public async Task Three_profiles_with_allergies_can_be_exported_then_permanently_deleted()
    {
        var (client, auth) = await factory.CreateSignedInClientAsync();
        var (bystander, bystanderAuth) = await factory.CreateSignedInClientAsync();
        using var _c = client;
        using var _b = bystander;
        var userId = auth.User.Id;

        var me = (await (await client.GetAsync("/api/v1/profiles")).ReadAsync<List<ProfileResponse>>()).Single();
        var mom = await (await client.PostJsonAsync("/api/v1/profiles", new SaveProfileRequest("Mom", "Mother", AgeGroup.Elderly, false, false))).ReadAsync<ProfileResponse>();
        var dad = await (await client.PostJsonAsync("/api/v1/profiles", new SaveProfileRequest("Dad", "Father", AgeGroup.Elderly, false, false))).ReadAsync<ProfileResponse>();
        foreach (var (profile, allergy) in new[] { (me, "Penicillin"), (mom, "Sulfa"), (dad, "Aspirin") })
            (await client.PostJsonAsync($"/api/v1/profiles/{profile.Id}/allergies", new AddAllergyRequest(null, allergy))).Dispose();
        (await bystander.PostJsonAsync("/api/v1/profiles", new SaveProfileRequest("Bystander's son", null, AgeGroup.Child, false, false))).Dispose();

        // A file the user owns, to prove blobs are deleted too.
        var blobs = factory.Services.GetRequiredService<IBlobStorage>();
        await using (var content = new MemoryStream(Encoding.UTF8.GetBytes("lab result")))
            await blobs.PutAsync($"{BlobKeys.UserPrefix(userId)}records/lab.pdf", content, "application/pdf", CancellationToken.None);
        await using (var content = new MemoryStream(Encoding.UTF8.GetBytes("other")))
            await blobs.PutAsync($"{BlobKeys.UserPrefix(bystanderAuth.User.Id)}records/x.pdf", content, "application/pdf", CancellationToken.None);

        // Export (BR-38)
        using var export = await client.GetAsync("/api/v1/me/export");
        await export.EnsureStatusAsync(HttpStatusCode.OK);
        Assert.Equal("no-store", export.Headers.CacheControl?.ToString());
        Assert.Equal("attachment", export.Content.Headers.ContentDisposition?.DispositionType);
        var data = await export.ReadAsync<AccountExport>();
        Assert.Equal(auth.User.Email, data.Account.Email);
        Assert.Equal(["Me", "Mom", "Dad"], data.Profiles.Select(p => p.DisplayName));
        Assert.Equal(["Penicillin", "Sulfa", "Aspirin"], data.Profiles.Select(p => p.Allergies.Single().AllergyText));
        Assert.DoesNotContain(data.Profiles, p => p.DisplayName == "Bystander's son");

        // Wrong password: nothing happens
        using var wrong = await client.PostJsonAsync("/api/v1/me/delete", new DeleteAccountRequest("not my password"));
        await wrong.AssertProblemAsync(HttpStatusCode.BadRequest, "wrong_password");
        Assert.True(await factory.WithDbAsync(db => db.Users.AnyAsync(u => u.Id == userId)));

        // Delete everything
        using var delete = await client.PostJsonAsync("/api/v1/me/delete", new DeleteAccountRequest(ApiClientExtensions.DefaultPassword));
        await delete.EnsureStatusAsync(HttpStatusCode.NoContent);

        await factory.WithDbAsync(async db =>
        {
            var profileIds = new[] { me.Id, mom.Id, dad.Id };
            Assert.False(await db.Users.AnyAsync(u => u.Id == userId));
            Assert.False(await db.UserRoles.AnyAsync(r => r.UserId == userId));
            Assert.False(await db.Profiles.AnyAsync(p => p.OwnerUserId == userId));
            Assert.False(await db.ProfileAllergies.AnyAsync(a => profileIds.Contains(a.ProfileId)));
            Assert.False(await db.RefreshTokens.AnyAsync(t => t.UserId == userId));
            // The other user is untouched.
            Assert.True(await db.Profiles.AnyAsync(p => p.OwnerUserId == bystanderAuth.User.Id && p.DisplayName == "Bystander's son"));
        });
        Assert.Null(await blobs.OpenReadAsync($"{BlobKeys.UserPrefix(userId)}records/lab.pdf", CancellationToken.None));
        await using (var other = await blobs.OpenReadAsync($"{BlobKeys.UserPrefix(bystanderAuth.User.Id)}records/x.pdf", CancellationToken.None))
            Assert.NotNull(other);

        // The old session is dead.
        using var refresh = await client.PostJsonAsync("/api/v1/auth/refresh", new RefreshRequest(auth.RefreshToken));
        await refresh.AssertProblemAsync(HttpStatusCode.Unauthorized, "invalid_refresh_token");
        using var login = await client.PostJsonAsync("/api/v1/auth/login", new LoginRequest(auth.User.Email, ApiClientExtensions.DefaultPassword));
        await login.AssertProblemAsync(HttpStatusCode.Unauthorized, "invalid_credentials");
    }

    [Fact]
    public async Task The_same_email_can_register_again_after_deletion()
    {
        var email = ApiClientExtensions.UniqueEmail();
        var (client, _) = await factory.CreateSignedInClientAsync(email);
        using var _c = client;
        (await client.PostJsonAsync("/api/v1/me/delete", new DeleteAccountRequest(ApiClientExtensions.DefaultPassword))).Dispose();

        using var fresh = factory.CreateClient();
        var again = await fresh.RegisterAsync(email);

        Assert.Equal(email, again.User.Email);
    }
}
