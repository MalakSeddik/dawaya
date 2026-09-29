using Dawaya.Api.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dawaya.Api.Data.Configurations.Catalog;

public class MedicineProductConfiguration : IEntityTypeConfiguration<MedicineProduct>
{
    public void Configure(EntityTypeBuilder<MedicineProduct> b)
    {
        b.ToTable("MedicineProducts");
        b.Property(x => x.Form).HasMaxLength(50).IsRequired();
        b.Property(x => x.StrengthText).HasMaxLength(100).IsRequired();
        b.Property(x => x.StrengthValue).HasPrecision(18, 4);
        b.Property(x => x.StrengthUnit).HasMaxLength(20);
        b.Property(x => x.Barcode).HasMaxLength(50);
        b.Property(x => x.UnitOfCount).HasMaxLength(20).IsRequired();

        b.HasOne(x => x.Medicine).WithMany(m => m.Products)
            .HasForeignKey(x => x.MedicineId).OnDelete(DeleteBehavior.Cascade);

        // EF adds "[Barcode] IS NOT NULL" automatically for SQL Server
        b.HasIndex(x => x.Barcode).IsUnique();
        b.HasIndex(x => new { x.MedicineId, x.Form, x.StrengthText }).IsUnique();
    }
}
