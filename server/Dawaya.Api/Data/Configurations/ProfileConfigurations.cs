using Dawaya.Api.Domain.Profiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dawaya.Api.Data.Configurations;

public class PatientProfileConfiguration : IEntityTypeConfiguration<PatientProfile>
{
    public void Configure(EntityTypeBuilder<PatientProfile> b)
    {
        b.ToTable("Profiles");
        b.Property(x => x.DisplayName).HasMaxLength(100).IsRequired();
        b.Property(x => x.Relationship).HasMaxLength(50);
        b.Property(x => x.AgeGroup).HasConversion<string>().HasMaxLength(20);

        // Cascade so "delete all my data" (BR-38) removes every profile with the account.
        b.HasOne(x => x.Owner).WithMany(u => u.Profiles)
            .HasForeignKey(x => x.OwnerUserId).OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(x => x.OwnerUserId);
        // At most one "myself" profile per account
        b.HasIndex(x => x.OwnerUserId).IsUnique().HasFilter("[IsSelf] = 1")
            .HasDatabaseName("UX_Profiles_OneSelfPerUser");
    }
}

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
