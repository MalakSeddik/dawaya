using Dawaya.Api.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dawaya.Api.Data.Configurations.Catalog;

public class ProductDoseRuleConfiguration : IEntityTypeConfiguration<ProductDoseRule>
{
    public void Configure(EntityTypeBuilder<ProductDoseRule> b)
    {
        b.ToTable("ProductDoseRules");
        b.Property(x => x.AgeGroup).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.AgeRangeText).HasMaxLength(100);
        b.Property(x => x.UsualDoseTextAr).HasMaxLength(500);
        b.Property(x => x.UsualDoseTextEn).HasMaxLength(500);
        b.Property(x => x.MaxDailyAmount).HasPrecision(18, 4);
        b.Property(x => x.MaxDailyUnit).HasMaxLength(20);

        b.HasOne(x => x.Product).WithMany(p => p.DoseRules)
            .HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(x => x.ProductId);
    }
}
