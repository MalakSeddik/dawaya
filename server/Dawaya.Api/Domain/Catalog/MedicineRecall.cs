namespace Dawaya.Api.Domain.Catalog;

public class MedicineRecall : BaseEntity
{
    public int MedicineId { get; set; }
    public Medicine Medicine { get; set; } = null!;
    public int? ProductId { get; set; }
    public MedicineProduct? Product { get; set; }

    public string? Batches { get; set; }
    public DateTime AnnouncedAt { get; set; }
    public string? SourceRef { get; set; }
    public string? Note { get; set; }
}
