using Dawaya.Api.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dawaya.Api.Data.Configurations.Catalog;

public class ActiveIngredientConfiguration : IEntityTypeConfiguration<ActiveIngredient>
{
    public void Configure(EntityTypeBuilder<ActiveIngredient> b)
    {
        b.ToTable("ActiveIngredients");
        b.Property(x => x.NameEn).HasMaxLength(200).IsRequired();
        b.Property(x => x.NameAr).HasMaxLength(200);
        b.HasIndex(x => x.NameEn).IsUnique();
    }
}
