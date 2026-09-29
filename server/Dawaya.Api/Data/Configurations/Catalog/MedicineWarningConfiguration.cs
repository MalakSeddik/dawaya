using Dawaya.Api.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dawaya.Api.Data.Configurations.Catalog;

public class MedicineWarningConfiguration : IEntityTypeConfiguration<MedicineWarning>
{
    public void Configure(EntityTypeBuilder<MedicineWarning> b)
    {
        b.ToTable("MedicineWarnings");
        b.Property(x => x.TargetGroup).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.Severity).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.TextAr).IsRequired();

        b.HasOne(x => x.Medicine).WithMany(m => m.Warnings)
            .HasForeignKey(x => x.MedicineId).OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(x => new { x.MedicineId, x.TargetGroup });
    }
}
