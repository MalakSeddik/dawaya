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

// Glossary for BR-17: plain explanation next to medical terms
public class MedicalTerm : BaseEntity
{
    public string TermAr { get; set; } = string.Empty;
    public string TermEn { get; set; } = string.Empty;
    public string PlainExplanationAr { get; set; } = string.Empty;
}

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
