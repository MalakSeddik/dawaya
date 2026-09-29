namespace Dawaya.Api.Domain.Catalog;

public class ActiveIngredient : BaseEntity
{
    public string NameEn { get; set; } = string.Empty;
    public string? NameAr { get; set; }

    public ICollection<ProductIngredient> Products { get; set; } = new List<ProductIngredient>();
}

public class ProductIngredient : BaseEntity
{
    public int ProductId { get; set; }
    public MedicineProduct Product { get; set; } = null!;
    public int ActiveIngredientId { get; set; }
    public ActiveIngredient ActiveIngredient { get; set; } = null!;
    public decimal? Amount { get; set; }
    public string? Unit { get; set; }
}
