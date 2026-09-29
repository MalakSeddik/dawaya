using Dawaya.Api.Data;
using Dawaya.Api.Domain.Identity;
using Dawaya.Api.Domain.Profiles;
using Dawaya.Api.Features.Account;
using Dawaya.Api.Infrastructure.Email;
using Dawaya.Api.Infrastructure.Results;
using Dawaya.Api.Infrastructure.Security;
using Microsoft.AspNetCore.Identity;

namespace Dawaya.Api.Features.Auth;

public interface IAuthService
{
    Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken ct);
    Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken ct);
    Task<Result<AuthResponse>> RefreshAsync(RefreshRequest request, CancellationToken ct);
    Task LogoutAsync(LogoutRequest request, CancellationToken ct);
    Task ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken ct);
    Task<Result> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct);
}

public sealed class AuthService(
    AppDbContext db,
    UserManager<ApplicationUser> users,
    IAccessTokenIssuer accessTokens,
    IRefreshTokenService refreshTokens,
    IEmailSender email,
    IConfiguration config,
    TimeProvider clock) : IAuthService
{
    private static readonly AppError InvalidCredentials =
        AppError.Unauthorized("invalid_credentials", "The email or password is incorrect.");
    private static readonly AppError AccountLocked =
        AppError.Unauthorized("account_locked", "Too many wrong attempts. Try again in 15 minutes.");

    public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken ct)
    {
        var emailAddress = request.Email.Trim();
        var user = new ApplicationUser
        {
            UserName = emailAddress,
            Email = emailAddress,
            PreferredLanguage = request.PreferredLanguage ?? "ar",
            CreatedAt = clock.GetUtcNow().UtcDateTime,
        };

        // The account, its role and the "Me" profile are created together or not at all.
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var created = await users.CreateAsync(user, request.Password);
        if (!created.Succeeded) return MapIdentityErrors(created);

        var role = await users.AddToRoleAsync(user, Roles.User);
        if (!role.Succeeded) throw new InvalidOperationException("The User role is missing; apply the migrations.");

        db.Profiles.Add(new PatientProfile { OwnerUserId = user.Id, DisplayName = request.DisplayName.Trim(), IsSelf = true });
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return await IssueAsync(user, familyId: null, ct);
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken ct)
    {
        var user = await users.FindByEmailAsync(request.Email.Trim());
        if (user is null) return InvalidCredentials;
        if (await users.IsLockedOutAsync(user)) return AccountLocked;

        if (!await users.CheckPasswordAsync(user, request.Password))
        {
            await users.AccessFailedAsync(user);
            return await users.IsLockedOutAsync(user) ? AccountLocked : InvalidCredentials;
        }

        await users.ResetAccessFailedCountAsync(user);
        return await IssueAsync(user, familyId: null, ct);
    }

    public async Task<Result<AuthResponse>> RefreshAsync(RefreshRequest request, CancellationToken ct)
    {
        var rotated = await refreshTokens.RotateAsync(request.RefreshToken, ct);
        if (!rotated.IsSuccess) return rotated.Error;

        var user = await users.FindByIdAsync(rotated.Value.UserId);
        if (user is null || await users.IsLockedOutAsync(user)) return RefreshTokenService.InvalidToken;

        return await BuildResponseAsync(user, rotated.Value.Token);
    }

    public Task LogoutAsync(LogoutRequest request, CancellationToken ct) =>
        refreshTokens.RevokeFamilyAsync(request.RefreshToken, ct);

    public async Task ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken ct)
    {
        // Same response whether or not the account exists, so emails cannot be discovered.
        var user = await users.FindByEmailAsync(request.Email.Trim());
        if (user?.Email is null) return;

        var token = await users.GeneratePasswordResetTokenAsync(user);
        var webBase = (config["App:WebBaseUrl"] ?? "http://localhost:5173").TrimEnd('/');
        var link = $"{webBase}/reset-password#uid={Uri.EscapeDataString(user.Id)}&token={Uri.EscapeDataString(token)}";
        await email.SendAsync(PasswordResetEmail.Create(user.Email, user.PreferredLanguage, link), ct);
    }

    public async Task<Result> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct)
    {
        var invalid = AppError.BadRequest("invalid_reset_link", "This reset link is invalid or has expired.");
        var user = await users.FindByIdAsync(request.UserId);
        if (user is null) return invalid;

        var reset = await users.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (!reset.Succeeded)
        {
            return reset.Errors.Any(e => e.Code == "InvalidToken") ? invalid : MapIdentityErrors(reset);
        }

        // A password reset signs the user out everywhere and clears any lockout.
        await refreshTokens.RevokeAllForUserAsync(user.Id, ct);
        await users.SetLockoutEndDateAsync(user, null);
        await users.ResetAccessFailedCountAsync(user);
        return Result.Success();
    }

    private async Task<AuthResponse> IssueAsync(ApplicationUser user, Guid? familyId, CancellationToken ct) =>
        await BuildResponseAsync(user, await refreshTokens.IssueAsync(user.Id, familyId, ct));

    private async Task<AuthResponse> BuildResponseAsync(ApplicationUser user, IssuedRefreshToken refresh)
    {
        var roles = await users.GetRolesAsync(user);
        var (access, accessExpires) = accessTokens.Issue(user.Id, roles);
        return new AuthResponse(access, accessExpires, refresh.RawToken, refresh.ExpiresAt, CurrentUserResponse.From(user, roles));
    }

    private static AppError MapIdentityErrors(IdentityResult result)
    {
        var codes = result.Errors.Select(e => e.Code).ToHashSet();
        if (codes.Contains("DuplicateEmail") || codes.Contains("DuplicateUserName"))
            return AppError.Conflict("email_taken", "An account with this email already exists.");
        if (codes.Contains("PasswordSameAsEmail"))
            return AppError.BadRequest("password_same_as_email", "The password must not be the same as the email.");
        if (codes.Any(c => c.StartsWith("Password", StringComparison.Ordinal)))
            return AppError.BadRequest("password_too_weak", "The password must be at least 8 characters.");
        if (codes.Contains("InvalidEmail") || codes.Contains("InvalidUserName"))
            return AppError.BadRequest("invalid_email", "The email address is not valid.");
        return AppError.BadRequest("registration_failed", "The account could not be created.");
    }
}
