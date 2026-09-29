using Dawaya.Api.Domain.Catalog;

namespace Dawaya.Api.Domain.Profiles;

public class ProfileAllergy : BaseEntity
{
    public int ProfileId { get; set; }
    public PatientProfile Profile { get; set; } = null!;

    // Mapped allergies trigger the red warning (BR-25). Unmapped ones stay as text.
    public int? ActiveIngredientId { get; set; }
    public ActiveIngredient? ActiveIngredient { get; set; }
    public string? AllergyText { get; set; }
}
