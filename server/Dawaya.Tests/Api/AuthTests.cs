using System.Net;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using Dawaya.Api.Features.Auth;
using Dawaya.Api.Features.Profiles;
using Dawaya.Api.Domain.Identity;
using Dawaya.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Dawaya.Tests.Api;

public sealed partial class AuthTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Register_creates_the_user_role_me_profile_and_tokens()
    {
        using var client = factory.CreateClient();
        var email = ApiClientExtensions.UniqueEmail();

        var auth = await client.RegisterAsync(email, displayName: "Malak");

        Assert.Equal(email, auth.User.Email);
        Assert.Equal(["User"], auth.User.Roles);
        Assert.Equal("ar", auth.User.PreferredLanguage);
        Assert.NotEmpty(auth.AccessToken);
        Assert.NotEmpty(auth.RefreshToken);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var profiles = await (await client.GetAsync("/api/v1/profiles")).ReadAsync<List<ProfileResponse>>();
        var me = Assert.Single(profiles);
        Assert.True(me.IsSelf);
        Assert.Equal("Malak", me.DisplayName);
    }

    [Fact]
    public async Task Register_keeps_the_display_settings_chosen_as_a_guest()
    {
        using var client = factory.CreateClient();

        using var response = await client.PostJsonAsync("/api/v1/auth/register",
            new RegisterRequest(ApiClientExtensions.UniqueEmail(), ApiClientExtensions.DefaultPassword, "Nana", "en", LargeText: true, HighContrast: true));

        var user = (await response.ReadAsync<AuthResponse>()).User;
        Assert.Equal("en", user.PreferredLanguage);
        Assert.True(user.LargeText);
        Assert.True(user.HighContrast);
    }

    [Fact]
    public async Task Access_token_lasts_15_minutes_and_carries_no_email()
    {
        using var client = factory.CreateClient();
        var auth = await client.RegisterAsync();

        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(auth.AccessToken);
        Assert.Equal(auth.User.Id, jwt.Subject);
        Assert.DoesNotContain(jwt.Claims, c => c.Value.Contains('@', StringComparison.Ordinal));
        Assert.Equal(15, (auth.AccessTokenExpiresAt - jwt.IssuedAt).TotalMinutes, precision: 0);
        Assert.Equal(30, (auth.RefreshTokenExpiresAt - jwt.IssuedAt).TotalDays, precision: 0);
    }

    [Fact]
    public async Task Register_with_an_existing_email_is_a_conflict()
    {
        using var client = factory.CreateClient();
        var email = ApiClientExtensions.UniqueEmail();
        await client.RegisterAsync(email);

        using var response = await client.PostJsonAsync("/api/v1/auth/register", new RegisterRequest(email.ToUpperInvariant(), "another password", "X", null));

        await response.AssertProblemAsync(HttpStatusCode.Conflict, "email_taken");
    }

    [Fact]
    public async Task Register_rejects_short_passwords_and_passwords_equal_to_the_email()
    {
        using var client = factory.CreateClient();
        var email = ApiClientExtensions.UniqueEmail();

        using var tooShort = await client.PostJsonAsync("/api/v1/auth/register", new RegisterRequest(email, "1234567", "X", null));
        Assert.Equal(HttpStatusCode.BadRequest, tooShort.StatusCode);

        using var sameAsEmail = await client.PostJsonAsync("/api/v1/auth/register", new RegisterRequest(email, email, "X", null));
        await sameAsEmail.AssertProblemAsync(HttpStatusCode.BadRequest, "password_same_as_email");

        // Nothing was created by the failed attempts (the whole registration is one transaction).
        Assert.False(await factory.WithDbAsync(db => db.Users.AnyAsync(u => u.Email == email)));
    }

    [Fact]
    public async Task Wrong_password_and_unknown_email_look_the_same()
    {
        using var client = factory.CreateClient();
        var auth = await client.RegisterAsync();

        using var wrong = await client.PostJsonAsync("/api/v1/auth/login", new LoginRequest(auth.User.Email, "wrong password"));
        await wrong.AssertProblemAsync(HttpStatusCode.Unauthorized, "invalid_credentials");

        using var unknown = await client.PostJsonAsync("/api/v1/auth/login", new LoginRequest(ApiClientExtensions.UniqueEmail(), "whatever1"));
        await unknown.AssertProblemAsync(HttpStatusCode.Unauthorized, "invalid_credentials");
    }

    [Fact]
    public async Task Five_wrong_passwords_lock_the_account()
    {
        using var client = factory.CreateClient();
        var auth = await client.RegisterAsync();

        for (var i = 0; i < 4; i++)
        {
            using var r = await client.PostJsonAsync("/api/v1/auth/login", new LoginRequest(auth.User.Email, "wrong password"));
            await r.AssertProblemAsync(HttpStatusCode.Unauthorized, "invalid_credentials");
        }
        using var fifth = await client.PostJsonAsync("/api/v1/auth/login", new LoginRequest(auth.User.Email, "wrong password"));
        await fifth.AssertProblemAsync(HttpStatusCode.Unauthorized, "account_locked");

        using var correct = await client.PostJsonAsync("/api/v1/auth/login", new LoginRequest(auth.User.Email, ApiClientExtensions.DefaultPassword));
        await correct.AssertProblemAsync(HttpStatusCode.Unauthorized, "account_locked");
    }

    [Fact]
    public async Task Refresh_rotates_the_token()
    {
        using var client = factory.CreateClient();
        var auth = await client.RegisterAsync();

        using var first = await client.PostJsonAsync("/api/v1/auth/refresh", new RefreshRequest(auth.RefreshToken));
        await first.EnsureStatusAsync(HttpStatusCode.OK);
        var rotated = await first.ReadAsync<AuthResponse>();
        Assert.NotEqual(auth.RefreshToken, rotated.RefreshToken);

        using var second = await client.PostJsonAsync("/api/v1/auth/refresh", new RefreshRequest(rotated.RefreshToken));
        await second.EnsureStatusAsync(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Reusing_a_rotated_token_revokes_the_whole_sign_in()
    {
        using var client = factory.CreateClient();
        var auth = await client.RegisterAsync();
        using var first = await client.PostJsonAsync("/api/v1/auth/refresh", new RefreshRequest(auth.RefreshToken));
        var rotated = await first.ReadAsync<AuthResponse>();

        // An attacker replays the old token...
        using var replay = await client.PostJsonAsync("/api/v1/auth/refresh", new RefreshRequest(auth.RefreshToken));
        await replay.AssertProblemAsync(HttpStatusCode.Unauthorized, "invalid_refresh_token");

        // ...so the legitimate new token stops working too.
        using var legit = await client.PostJsonAsync("/api/v1/auth/refresh", new RefreshRequest(rotated.RefreshToken));
        await legit.AssertProblemAsync(HttpStatusCode.Unauthorized, "invalid_refresh_token");
    }

    [Fact]
    public async Task Other_sign_ins_are_not_affected_by_reuse_in_one_family()
    {
        using var client = factory.CreateClient();
        var auth = await client.RegisterAsync();
        using var login = await client.PostJsonAsync("/api/v1/auth/login", new LoginRequest(auth.User.Email, ApiClientExtensions.DefaultPassword));
        var otherDevice = await login.ReadAsync<AuthResponse>();

        using var first = await client.PostJsonAsync("/api/v1/auth/refresh", new RefreshRequest(auth.RefreshToken));
        using var replay = await client.PostJsonAsync("/api/v1/auth/refresh", new RefreshRequest(auth.RefreshToken));

        using var other = await client.PostJsonAsync("/api/v1/auth/refresh", new RefreshRequest(otherDevice.RefreshToken));
        await other.EnsureStatusAsync(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Logout_revokes_the_refresh_token()
    {
        using var client = factory.CreateClient();
        var auth = await client.RegisterAsync();

        using var logout = await client.PostJsonAsync("/api/v1/auth/logout", new LogoutRequest(auth.RefreshToken));
        await logout.EnsureStatusAsync(HttpStatusCode.NoContent);

        using var refresh = await client.PostJsonAsync("/api/v1/auth/refresh", new RefreshRequest(auth.RefreshToken));
        await refresh.AssertProblemAsync(HttpStatusCode.Unauthorized, "invalid_refresh_token");
    }

    [Fact]
    public async Task Expired_refresh_tokens_are_rejected_and_later_cleaned_up()
    {
        using var client = factory.CreateClient();
        var auth = await client.RegisterAsync();
        var hash = Dawaya.Api.Infrastructure.Security.TokenHashing.Sha256Hex(auth.RefreshToken);

        // Move the stored expiry into the past instead of moving the shared clock.
        await factory.WithDbAsync(db => db.RefreshTokens.Where(t => t.TokenHash == hash)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.ExpiresAt, DateTime.UtcNow.AddDays(-30))));

        using var refresh = await client.PostJsonAsync("/api/v1/auth/refresh", new RefreshRequest(auth.RefreshToken));
        await refresh.AssertProblemAsync(HttpStatusCode.Unauthorized, "invalid_refresh_token");

        await factory.WithDbAsync(async db =>
        {
            var removed = await new RefreshTokenService(db, Microsoft.Extensions.Options.Options.Create(
                new Dawaya.Api.Infrastructure.Security.JwtOptions()), TimeProvider.System).DeleteStaleAsync(CancellationToken.None);
            Assert.True(removed >= 1);
            Assert.False(await db.RefreshTokens.AnyAsync(t => t.TokenHash == hash));
        });
    }

    [Fact]
    public async Task Forgot_password_for_an_unknown_email_sends_nothing_but_looks_the_same()
    {
        using var client = factory.CreateClient();
        var before = EmailFiles().Length;

        using var response = await client.PostJsonAsync("/api/v1/auth/forgot-password", new ForgotPasswordRequest(ApiClientExtensions.UniqueEmail()));

        await response.EnsureStatusAsync(HttpStatusCode.Accepted);
        Assert.Equal(before, EmailFiles().Length);
    }

    [Fact]
    public async Task Password_reset_flow_changes_the_password_and_signs_out_everywhere()
    {
        using var client = factory.CreateClient();
        var auth = await client.RegisterAsync();

        using var forgot = await client.PostJsonAsync("/api/v1/auth/forgot-password", new ForgotPasswordRequest(auth.User.Email));
        await forgot.EnsureStatusAsync(HttpStatusCode.Accepted);

        var email = EmailFiles().Select(File.ReadAllText).Single(t => t.Contains($"To: {auth.User.Email}", StringComparison.Ordinal));
        var link = ResetLink().Match(email);
        Assert.True(link.Success, "The email should contain a reset link with the token in the URL fragment.");
        Assert.StartsWith("https://app.test/reset-password#", link.Value, StringComparison.Ordinal);
        var userId = Uri.UnescapeDataString(link.Groups["uid"].Value);
        var token = Uri.UnescapeDataString(link.Groups["token"].Value);

        using var reset = await client.PostJsonAsync("/api/v1/auth/reset-password", new ResetPasswordRequest(userId, token, "a brand new password"));
        await reset.EnsureStatusAsync(HttpStatusCode.NoContent);

        using var oldSession = await client.PostJsonAsync("/api/v1/auth/refresh", new RefreshRequest(auth.RefreshToken));
        await oldSession.AssertProblemAsync(HttpStatusCode.Unauthorized, "invalid_refresh_token");

        using var login = await client.PostJsonAsync("/api/v1/auth/login", new LoginRequest(auth.User.Email, "a brand new password"));
        await login.EnsureStatusAsync(HttpStatusCode.OK);

        // The link works once.
        using var again = await client.PostJsonAsync("/api/v1/auth/reset-password", new ResetPasswordRequest(userId, token, "yet another password"));
        await again.AssertProblemAsync(HttpStatusCode.BadRequest, "invalid_reset_link");
    }

    [Fact]
    public async Task Reset_with_a_bad_token_is_rejected()
    {
        using var client = factory.CreateClient();
        var auth = await client.RegisterAsync();

        using var reset = await client.PostJsonAsync("/api/v1/auth/reset-password", new ResetPasswordRequest(auth.User.Id, "not-a-token", "a brand new password"));

        await reset.AssertProblemAsync(HttpStatusCode.BadRequest, "invalid_reset_link");
    }

    [Fact]
    public async Task Refresh_tokens_are_stored_only_as_hashes()
    {
        using var client = factory.CreateClient();
        var auth = await client.RegisterAsync();

        var stored = await factory.WithDbAsync(db => db.RefreshTokens.Where(t => t.UserId == auth.User.Id).Select(t => t.TokenHash).ToListAsync());

        var hash = Assert.Single(stored);
        Assert.NotEqual(auth.RefreshToken, hash);
        Assert.Equal(64, hash.Length);
    }

    [Fact]
    public async Task Tokens_and_passwords_never_reach_the_logs()
    {
        using var client = factory.CreateClient();
        var auth = await client.RegisterAsync();
        using var refresh = await client.PostJsonAsync("/api/v1/auth/refresh", new RefreshRequest(auth.RefreshToken));

        Assert.DoesNotContain(factory.Logs.Messages, m => m.Contains(auth.RefreshToken, StringComparison.Ordinal));
        Assert.DoesNotContain(factory.Logs.Messages, m => m.Contains(auth.AccessToken, StringComparison.Ordinal));
        Assert.DoesNotContain(factory.Logs.Messages, m => m.Contains(ApiClientExtensions.DefaultPassword, StringComparison.Ordinal));
    }

    [Fact]
    public async Task Roles_are_seeded()
    {
        var names = await factory.WithDbAsync(db => db.Roles.Select(r => r.Name!).ToListAsync());
        Assert.Equal([Roles.Admin, Roles.Pharmacist, Roles.User], names.Order());
    }

    private string[] EmailFiles() =>
        Directory.Exists(factory.EmailDirectory) ? Directory.GetFiles(factory.EmailDirectory) : [];

    [GeneratedRegex(@"https://app\.test/reset-password#uid=(?<uid>[^&\s]+)&token=(?<token>\S+)")]
    private static partial Regex ResetLink();
}
