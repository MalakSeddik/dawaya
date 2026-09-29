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
