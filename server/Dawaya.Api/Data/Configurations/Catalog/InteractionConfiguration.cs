using Dawaya.Api.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dawaya.Api.Data.Configurations.Catalog;

public class InteractionConfiguration : IEntityTypeConfiguration<Interaction>
{
    public void Configure(EntityTypeBuilder<Interaction> b)
    {
        b.ToTable("Interactions", t =>
            t.HasCheckConstraint("CK_Interactions_IngredientOrder", "[IngredientAId] < [IngredientBId]"));

        b.Property(x => x.Severity).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.DescriptionAr).IsRequired();
        b.Property(x => x.Source).HasMaxLength(300);

        // Two FKs to the same table: NoAction to avoid multiple cascade paths
        b.HasOne(x => x.IngredientA).WithMany()
            .HasForeignKey(x => x.IngredientAId).OnDelete(DeleteBehavior.NoAction);
        b.HasOne(x => x.IngredientB).WithMany()
            .HasForeignKey(x => x.IngredientBId).OnDelete(DeleteBehavior.NoAction);

        b.HasIndex(x => new { x.IngredientAId, x.IngredientBId }).IsUnique();
        b.HasIndex(x => x.IngredientBId);
    }
}
