using Dawaya.Api.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dawaya.Api.Data.Configurations.Catalog;

public class MedicalTermConfiguration : IEntityTypeConfiguration<MedicalTerm>
{
    public void Configure(EntityTypeBuilder<MedicalTerm> b)
    {
        b.ToTable("MedicalTerms");
        b.Property(x => x.TermAr).HasMaxLength(200).IsRequired();
        b.Property(x => x.TermEn).HasMaxLength(200).IsRequired();
        b.Property(x => x.PlainExplanationAr).HasMaxLength(1000).IsRequired();
        b.HasIndex(x => x.TermEn);
        b.HasIndex(x => x.TermAr);
    }
}
