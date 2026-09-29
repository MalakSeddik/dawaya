using Dawaya.Api.Domain;

namespace Dawaya.Api.Features.Profiles;

public sealed record ProfileResponse(
    int Id,
    string DisplayName,
    string? Relationship,
    bool IsSelf,
    AgeGroup? AgeGroup,
    bool IsPregnant,
    bool IsBreastfeeding,
    IReadOnlyList<AllergyResponse> Allergies,
    DateTime CreatedAt,
    DateTime UpdatedAt);

// IngredientId is set when the allergy is linked to a database ingredient (drives the red warning, BR-25).
public sealed record AllergyResponse(int Id, int? IngredientId, string? IngredientNameEn, string? IngredientNameAr, string? AllergyText);

// BR-07: these details only ever affect warnings, never the dose shown.
public sealed record SaveProfileRequest(
    string DisplayName,
    string? Relationship,
    AgeGroup? AgeGroup,
    bool IsPregnant,
    bool IsBreastfeeding);

// Either pick an ingredient from the suggestions, or type the allergy as text.
public sealed record AddAllergyRequest(int? IngredientId, string? AllergyText);
