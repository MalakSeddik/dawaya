using Dawaya.Api.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dawaya.Api.Data.Configurations.Catalog;

public class MedicineConfiguration : IEntityTypeConfiguration<Medicine>
{
    public void Configure(EntityTypeBuilder<Medicine> b)
    {
        b.ToTable("Medicines");
        b.Property(x => x.BrandNameEn).HasMaxLength(200).IsRequired();
        b.Property(x => x.BrandNameAr).HasMaxLength(200);
        b.Property(x => x.Manufacturer).HasMaxLength(200);
        b.Property(x => x.RegistrationStatus).HasConversion<string>().HasMaxLength(20);

        // Autocomplete in Arabic and English (BR-80)
        b.HasIndex(x => x.BrandNameEn);
        b.HasIndex(x => x.BrandNameAr);
    }
}
