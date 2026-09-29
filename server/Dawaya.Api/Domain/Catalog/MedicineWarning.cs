namespace Dawaya.Api.Domain.Catalog;

public class MedicineWarning : BaseEntity
{
    public int MedicineId { get; set; }
    public Medicine Medicine { get; set; } = null!;

    public WarningTargetGroup TargetGroup { get; set; }
    public WarningSeverity Severity { get; set; }
    public string TextAr { get; set; } = string.Empty;
    public string? TextEn { get; set; }
    public ContentStatus Status { get; set; } = ContentStatus.Unverified;
}
