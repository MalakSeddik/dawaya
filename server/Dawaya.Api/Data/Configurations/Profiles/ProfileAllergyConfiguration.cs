using Dawaya.Api.Domain.Profiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dawaya.Api.Data.Configurations.Profiles;

public class ProfileAllergyConfiguration : IEntityTypeConfiguration<ProfileAllergy>
{
    public void Configure(EntityTypeBuilder<ProfileAllergy> b)
    {
        b.ToTable("ProfileAllergies");
        b.Property(x => x.AllergyText).HasMaxLength(200);

        b.HasOne(x => x.Profile).WithMany(p => p.Allergies)
            .HasForeignKey(x => x.ProfileId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.ActiveIngredient).WithMany()
            .HasForeignKey(x => x.ActiveIngredientId).OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => x.ProfileId);
        b.HasIndex(x => x.ActiveIngredientId);
        b.ToTable(t => t.HasCheckConstraint("CK_ProfileAllergies_HasValue",
            "[ActiveIngredientId] IS NOT NULL OR [AllergyText] IS NOT NULL"));
    }
}
