namespace Dawaya.Api.Domain.Catalog;

// Glossary for BR-17: plain explanation next to medical terms
public class MedicalTerm : BaseEntity
{
    public string TermAr { get; set; } = string.Empty;
    public string TermEn { get; set; } = string.Empty;
    public string PlainExplanationAr { get; set; } = string.Empty;
}
