using Dawaya.Api.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dawaya.Api.Data.Configurations.Catalog;

public class MedicineContentConfiguration : IEntityTypeConfiguration<MedicineContent>
{
    public void Configure(EntityTypeBuilder<MedicineContent> b)
    {
        b.ToTable("MedicineContents");
        b.Property(x => x.Language).HasConversion<string>().HasMaxLength(5);
        b.Property(x => x.Section).HasConversion<string>().HasMaxLength(30);
        b.Property(x => x.SourceType).HasConversion<string>().HasMaxLength(30);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.Body).IsRequired();
        b.Property(x => x.SourceRef).HasMaxLength(500);

        b.HasOne(x => x.Medicine).WithMany(m => m.Contents)
            .HasForeignKey(x => x.MedicineId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.VerifiedBy).WithMany()
            .HasForeignKey(x => x.VerifiedById).OnDelete(DeleteBehavior.NoAction);

        b.HasIndex(x => new { x.MedicineId, x.Language, x.Section, x.Version }).IsUnique();
    }
}
