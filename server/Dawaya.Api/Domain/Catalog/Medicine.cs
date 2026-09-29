namespace Dawaya.Api.Domain.Catalog;

public class Medicine : BaseEntity
{
    public string BrandNameEn { get; set; } = string.Empty;
    public string? BrandNameAr { get; set; }
    public string? Manufacturer { get; set; }
    public RegistrationStatus RegistrationStatus { get; set; } = RegistrationStatus.Registered; // BR-03
    public bool IsControlled { get; set; }        // BR-27
    public bool IsPrescriptionOnly { get; set; }  // BR-71
    public bool NoSuddenStop { get; set; }        // BR-61
    public bool IsRecalled { get; set; }          // BR-42

    public ICollection<MedicineProduct> Products { get; set; } = new List<MedicineProduct>();
    public ICollection<MedicineContent> Contents { get; set; } = new List<MedicineContent>();
    public ICollection<MedicineWarning> Warnings { get; set; } = new List<MedicineWarning>();
    public ICollection<MedicineRecall> Recalls { get; set; } = new List<MedicineRecall>();
}

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
