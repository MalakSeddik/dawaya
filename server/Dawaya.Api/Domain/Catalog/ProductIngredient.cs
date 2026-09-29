namespace Dawaya.Api.Domain.Catalog;

public class ProductIngredient : BaseEntity
{
    public int ProductId { get; set; }
    public MedicineProduct Product { get; set; } = null!;
    public int ActiveIngredientId { get; set; }
    public ActiveIngredient ActiveIngredient { get; set; } = null!;
    public decimal? Amount { get; set; }
    public string? Unit { get; set; }
}
