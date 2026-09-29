namespace Dawaya.Api.Domain.Catalog;

// Stored once per pair with IngredientAId < IngredientBId. No row = "no data available", never "safe" (BR-30).
public class Interaction : BaseEntity
{
    public int IngredientAId { get; set; }
    public ActiveIngredient IngredientA { get; set; } = null!;
    public int IngredientBId { get; set; }
    public ActiveIngredient IngredientB { get; set; } = null!;

    public InteractionSeverity Severity { get; set; }
    public string DescriptionAr { get; set; } = string.Empty;
    public string? DescriptionEn { get; set; }
    public string? Source { get; set; }
}
