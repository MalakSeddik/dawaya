using Dawaya.Api.Domain.Identity;
using Dawaya.Api.Features.Profiles;
using Dawaya.Api.Infrastructure.Results;
using Dawaya.Api.Infrastructure.Security;
using Dawaya.Api.Infrastructure.Storage;
using Microsoft.AspNetCore.Identity;

namespace Dawaya.Api.Features.Account;

public interface IAccountService
{
    Task<Result<CurrentUserResponse>> GetCurrentAsync(CancellationToken ct);
    Task<Result<CurrentUserResponse>> UpdatePreferencesAsync(UpdatePreferencesRequest request, CancellationToken ct);
    Task<Result<AccountExport>> ExportAsync(CancellationToken ct);
    Task<Result> DeleteAsync(DeleteAccountRequest request, CancellationToken ct);
}

public sealed class AccountService(
    UserManager<ApplicationUser> users,
    ICurrentUser currentUser,
    IProfileService profiles,
    IBlobStorage blobs,
    TimeProvider clock) : IAccountService
{
    public async Task<Result<CurrentUserResponse>> GetCurrentAsync(CancellationToken ct)
    {
        var user = await users.FindByIdAsync(currentUser.Id);
        if (user is null) return AppError.NotFound("account_not_found");
        return CurrentUserResponse.From(user, await users.GetRolesAsync(user));
    }

    public async Task<Result<CurrentUserResponse>> UpdatePreferencesAsync(UpdatePreferencesRequest request, CancellationToken ct)
    {
        var user = await users.FindByIdAsync(currentUser.Id);
        if (user is null) return AppError.NotFound("account_not_found");

        user.PreferredLanguage = request.PreferredLanguage;
        user.LargeText = request.LargeText;
        user.HighContrast = request.HighContrast;
        await users.UpdateAsync(user);
        return CurrentUserResponse.From(user, await users.GetRolesAsync(user));
    }

    public async Task<Result<AccountExport>> ExportAsync(CancellationToken ct)
    {
        var user = await users.FindByIdAsync(currentUser.Id);
        if (user is null) return AppError.NotFound("account_not_found");

        var roles = await users.GetRolesAsync(user);
        return new AccountExport(
            clock.GetUtcNow().UtcDateTime,
            new AccountExportUser(user.Email ?? string.Empty, user.CreatedAt, user.PreferredLanguage, user.LargeText, user.HighContrast, [.. roles]),
            await profiles.ListAsync(ct));
    }

    // BR-38: permanent delete. Blobs go first, so a failure part-way can simply be retried.
    // The database cascade then removes the user, profiles, allergies, refresh tokens and Identity rows.
    public async Task<Result> DeleteAsync(DeleteAccountRequest request, CancellationToken ct)
    {
        var user = await users.FindByIdAsync(currentUser.Id);
        if (user is null) return AppError.NotFound("account_not_found");
        if (!await users.CheckPasswordAsync(user, request.Password))
            return AppError.BadRequest("wrong_password", "The password is incorrect.");

        await blobs.DeletePrefixAsync(BlobKeys.UserPrefix(user.Id), ct);

        var deleted = await users.DeleteAsync(user);
        if (!deleted.Succeeded) throw new InvalidOperationException("The account could not be deleted.");
        return Result.Success();
    }
}
