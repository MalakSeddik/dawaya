using Dawaya.Api.Domain.Identity;
using Dawaya.Api.Features.Profiles;

namespace Dawaya.Api.Features.Account;

public sealed record CurrentUserResponse(
    string Id,
    string Email,
    string PreferredLanguage,
    bool LargeText,
    bool HighContrast,
    IReadOnlyList<string> Roles)
{
    public static CurrentUserResponse From(ApplicationUser user, IEnumerable<string> roles) =>
        new(user.Id, user.Email ?? string.Empty, user.PreferredLanguage, user.LargeText, user.HighContrast, [.. roles]);
}

// Accessibility and language preferences follow the user across devices (BR-33).
public sealed record UpdatePreferencesRequest(string PreferredLanguage, bool LargeText, bool HighContrast);

public sealed record DeleteAccountRequest(string Password);

// BR-38: everything the app stores about the user. Later slices add their data here.
public sealed record AccountExport(
    DateTime ExportedAt,
    AccountExportUser Account,
    IReadOnlyList<ProfileResponse> Profiles);

public sealed record AccountExportUser(
    string Email,
    DateTime CreatedAt,
    string PreferredLanguage,
    bool LargeText,
    bool HighContrast,
    IReadOnlyList<string> Roles);
