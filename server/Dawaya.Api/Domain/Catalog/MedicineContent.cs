using Dawaya.Api.Domain.Identity;

namespace Dawaya.Api.Domain.Catalog;

public class MedicineContent : BaseEntity
{
    public int MedicineId { get; set; }
    public Medicine Medicine { get; set; } = null!;

    public ContentLanguage Language { get; set; }
    public ContentSection Section { get; set; }
    public string Body { get; set; } = string.Empty;

    // BR-13, BR-14: every screen shows source and last-updated date
    public SourceType SourceType { get; set; }
    public string? SourceRef { get; set; }
    public DateTime SourceUpdatedAt { get; set; }

    // BR-15: AI-simplified text must be verified by an admin
    public bool IsAiSimplified { get; set; }
    public ContentStatus Status { get; set; } = ContentStatus.Unverified;
    public string? VerifiedById { get; set; }
    public ApplicationUser? VerifiedBy { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public int Version { get; set; } = 1;
}
