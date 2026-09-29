namespace Dawaya.Api.Domain.Catalog;

// Leaflet's usual dose per age group (BR-19, BR-22) and the max used for the typo check (BR-85).
public class ProductDoseRule : BaseEntity
{
    public int ProductId { get; set; }
    public MedicineProduct Product { get; set; } = null!;

    public AgeGroup AgeGroup { get; set; }
    public string? AgeRangeText { get; set; }
    public string? UsualDoseTextAr { get; set; }
    public string? UsualDoseTextEn { get; set; }
    // Null when the leaflet is weight-based. The typo check is skipped for Infant/Child anyway.
    public decimal? MaxDailyAmount { get; set; }
    public string? MaxDailyUnit { get; set; }
}
