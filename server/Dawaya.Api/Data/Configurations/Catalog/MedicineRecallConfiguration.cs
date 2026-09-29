using Dawaya.Api.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dawaya.Api.Data.Configurations.Catalog;

public class MedicineRecallConfiguration : IEntityTypeConfiguration<MedicineRecall>
{
    public void Configure(EntityTypeBuilder<MedicineRecall> b)
    {
        b.ToTable("MedicineRecalls");
        b.Property(x => x.Batches).HasMaxLength(500);
        b.Property(x => x.SourceRef).HasMaxLength(500);
        b.Property(x => x.Note).HasMaxLength(1000);

        b.HasOne(x => x.Medicine).WithMany(m => m.Recalls)
            .HasForeignKey(x => x.MedicineId).OnDelete(DeleteBehavior.Cascade);
        // NoAction avoids multiple cascade paths (Medicine -> Product -> Recall)
        b.HasOne(x => x.Product).WithMany()
            .HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.NoAction);
    }
}
