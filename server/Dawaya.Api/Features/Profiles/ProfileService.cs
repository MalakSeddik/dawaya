using Dawaya.Api.Data;
using Dawaya.Api.Domain.Profiles;
using Dawaya.Api.Infrastructure.Results;
using Dawaya.Api.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace Dawaya.Api.Features.Profiles;

public interface IProfileService
{
    Task<IReadOnlyList<ProfileResponse>> ListAsync(CancellationToken ct);
    Task<Result<ProfileResponse>> GetAsync(int id, CancellationToken ct);
    Task<Result<ProfileResponse>> CreateAsync(SaveProfileRequest request, CancellationToken ct);
    Task<Result<ProfileResponse>> UpdateAsync(int id, SaveProfileRequest request, CancellationToken ct);
    Task<Result> DeleteAsync(int id, CancellationToken ct);
    Task<Result<ProfileResponse>> AddAllergyAsync(int profileId, AddAllergyRequest request, CancellationToken ct);
    Task<Result> RemoveAllergyAsync(int profileId, int allergyId, CancellationToken ct);
}

// Every query is scoped to the signed-in owner. Another user's profile is reported as "not found"
// (never "forbidden"), so ids of other people's profiles cannot be probed.
public sealed class ProfileService(AppDbContext db, ICurrentUser currentUser) : IProfileService
{
    public const int MaxProfilesPerAccount = 20;

    private IQueryable<PatientProfile> Owned => db.Profiles.Where(p => p.OwnerUserId == currentUser.Id);

    public async Task<IReadOnlyList<ProfileResponse>> ListAsync(CancellationToken ct)
    {
        var profiles = await Owned.AsNoTracking()
            .Include(p => p.Allergies).ThenInclude(a => a.ActiveIngredient)
            .OrderByDescending(p => p.IsSelf).ThenBy(p => p.CreatedAt).ThenBy(p => p.Id)
            .ToListAsync(ct);
        return [.. profiles.Select(ToResponse)];
    }

    public async Task<Result<ProfileResponse>> GetAsync(int id, CancellationToken ct)
    {
        var profile = await LoadAsync(id, ct);
        return profile is null ? AppError.NotFound("profile_not_found") : ToResponse(profile);
    }

    public async Task<Result<ProfileResponse>> CreateAsync(SaveProfileRequest request, CancellationToken ct)
    {
        if (await Owned.CountAsync(ct) >= MaxProfilesPerAccount)
            return AppError.Conflict("profile_limit_reached", $"An account can have at most {MaxProfilesPerAccount} profiles.");

        var profile = new PatientProfile { OwnerUserId = currentUser.Id };
        Apply(profile, request);
        db.Profiles.Add(profile);
        await db.SaveChangesAsync(ct);
        return ToResponse(profile);
    }

    public async Task<Result<ProfileResponse>> UpdateAsync(int id, SaveProfileRequest request, CancellationToken ct)
    {
        var profile = await LoadAsync(id, ct);
        if (profile is null) return AppError.NotFound("profile_not_found");

        Apply(profile, request);
        await db.SaveChangesAsync(ct);
        return ToResponse(profile);
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken ct)
    {
        var profile = await Owned.SingleOrDefaultAsync(p => p.Id == id, ct);
        if (profile is null) return AppError.NotFound("profile_not_found");
        if (profile.IsSelf)
            return AppError.Conflict("self_profile_cannot_be_deleted", "Your own profile is only removed when you delete your account.");

        db.Profiles.Remove(profile);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result<ProfileResponse>> AddAllergyAsync(int profileId, AddAllergyRequest request, CancellationToken ct)
    {
        var profile = await LoadAsync(profileId, ct);
        if (profile is null) return AppError.NotFound("profile_not_found");

        var text = string.IsNullOrWhiteSpace(request.AllergyText) ? null : request.AllergyText.Trim();
        var ingredientId = request.IngredientId;
        if (ingredientId is not null)
        {
            if (!await db.ActiveIngredients.AnyAsync(i => i.Id == ingredientId, ct))
                return AppError.BadRequest("ingredient_not_found", "The chosen ingredient does not exist.");
        }
        else if (text is not null)
        {
            // Typed text that exactly matches an ingredient is linked, so the red allergy warning can fire (BR-25).
            var lowered = text.ToLowerInvariant();
            // Runs as SQL (LOWER), so .NET culture rules do not apply.
#pragma warning disable CA1304, CA1311, CA1862
            ingredientId = await db.ActiveIngredients
                .Where(i => i.NameEn.ToLower() == lowered || (i.NameAr != null && i.NameAr == text))
                .Select(i => (int?)i.Id)
                .FirstOrDefaultAsync(ct);
#pragma warning restore CA1304, CA1311, CA1862
        }

        var duplicate = ingredientId is not null
            ? profile.Allergies.Any(a => a.ActiveIngredientId == ingredientId)
            : profile.Allergies.Any(a => a.ActiveIngredientId is null && string.Equals(a.AllergyText, text, StringComparison.OrdinalIgnoreCase));
        if (duplicate) return AppError.Conflict("allergy_already_added", "This allergy is already on the profile.");

        profile.Allergies.Add(new ProfileAllergy { ActiveIngredientId = ingredientId, AllergyText = text });
        await db.SaveChangesAsync(ct);

        return ToResponse((await LoadAsync(profileId, ct))!);
    }

    public async Task<Result> RemoveAllergyAsync(int profileId, int allergyId, CancellationToken ct)
    {
        var allergy = await db.ProfileAllergies
            .SingleOrDefaultAsync(a => a.Id == allergyId && a.ProfileId == profileId && a.Profile.OwnerUserId == currentUser.Id, ct);
        if (allergy is null) return AppError.NotFound("allergy_not_found");

        db.ProfileAllergies.Remove(allergy);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }

    private Task<PatientProfile?> LoadAsync(int id, CancellationToken ct) =>
        Owned.Include(p => p.Allergies).ThenInclude(a => a.ActiveIngredient).SingleOrDefaultAsync(p => p.Id == id, ct);

    private static void Apply(PatientProfile profile, SaveProfileRequest request)
    {
        profile.DisplayName = request.DisplayName.Trim();
        profile.Relationship = string.IsNullOrWhiteSpace(request.Relationship) ? null : request.Relationship.Trim();
        profile.AgeGroup = request.AgeGroup;
        profile.IsPregnant = request.IsPregnant;
        profile.IsBreastfeeding = request.IsBreastfeeding;
    }

    private static ProfileResponse ToResponse(PatientProfile p) => new(
        p.Id, p.DisplayName, p.Relationship, p.IsSelf, p.AgeGroup, p.IsPregnant, p.IsBreastfeeding,
        [.. p.Allergies.OrderBy(a => a.Id).Select(a => new AllergyResponse(
            a.Id, a.ActiveIngredientId, a.ActiveIngredient?.NameEn, a.ActiveIngredient?.NameAr, a.AllergyText))],
        p.CreatedAt, p.UpdatedAt);
}
