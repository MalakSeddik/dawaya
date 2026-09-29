namespace Dawaya.Api.Domain.Catalog;

// One row per strength + form (BR-11, BR-81). Profile medications point here, never just at a brand.
public class MedicineProduct : BaseEntity
{
    public int MedicineId { get; set; }
    public Medicine Medicine { get; set; } = null!;

    public string Form { get; set; } = string.Empty;          // tablet, syrup...
    public string StrengthText { get; set; } = string.Empty;  // "500mg", "250mg/5ml"
    public decimal? StrengthValue { get; set; }
    public string? StrengthUnit { get; set; }
    public string? Barcode { get; set; }
    // tablet, ml... Pack quantity and dose must use this unit so refill math works.
    public string UnitOfCount { get; set; } = "tablet";

    public ICollection<ProductIngredient> Ingredients { get; set; } = new List<ProductIngredient>();
    public ICollection<ProductDoseRule> DoseRules { get; set; } = new List<ProductDoseRule>();
}
