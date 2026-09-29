using Dawaya.Api.Domain.Catalog;
using Dawaya.Api.Domain.Identity;

namespace Dawaya.Api.Domain.Profiles;

// Table name is "Profiles". Class is PatientProfile to avoid clashing with AutoMapper.Profile.
// These fields affect warnings only, never dosage (BR-07).
public class PatientProfile : BaseEntity
{
    public string OwnerUserId { get; set; } = string.Empty;
    public ApplicationUser Owner { get; set; } = null!;

    public string DisplayName { get; set; } = string.Empty;
    public string? Relationship { get; set; }
    public bool IsSelf { get; set; }
    public AgeGroup? AgeGroup { get; set; }
    public bool IsPregnant { get; set; }
    public bool IsBreastfeeding { get; set; }

    public ICollection<ProfileAllergy> Allergies { get; set; } = new List<ProfileAllergy>();
}

public class ProfileAllergy : BaseEntity
{
    public int ProfileId { get; set; }
    public PatientProfile Profile { get; set; } = null!;

    // Mapped allergies trigger the red warning (BR-25). Unmapped ones stay as text.
    public int? ActiveIngredientId { get; set; }
    public ActiveIngredient? ActiveIngredient { get; set; }
    public string? AllergyText { get; set; }
}
