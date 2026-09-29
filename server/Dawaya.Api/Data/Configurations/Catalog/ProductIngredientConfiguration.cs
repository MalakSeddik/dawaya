using Dawaya.Api.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dawaya.Api.Data.Configurations.Catalog;

public class ProductIngredientConfiguration : IEntityTypeConfiguration<ProductIngredient>
{
    public void Configure(EntityTypeBuilder<ProductIngredient> b)
    {
        b.ToTable("ProductIngredients");
        b.Property(x => x.Amount).HasPrecision(18, 4);
        b.Property(x => x.Unit).HasMaxLength(20);

        b.HasOne(x => x.Product).WithMany(p => p.Ingredients)
            .HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.ActiveIngredient).WithMany(i => i.Products)
            .HasForeignKey(x => x.ActiveIngredientId).OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => new { x.ProductId, x.ActiveIngredientId }).IsUnique();
    }
}
