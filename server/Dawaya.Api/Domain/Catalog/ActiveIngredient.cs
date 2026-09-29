namespace Dawaya.Api.Domain.Catalog;

public class ActiveIngredient : BaseEntity
{
    public string NameEn { get; set; } = string.Empty;
    public string? NameAr { get; set; }

    public ICollection<ProductIngredient> Products { get; set; } = new List<ProductIngredient>();
}
