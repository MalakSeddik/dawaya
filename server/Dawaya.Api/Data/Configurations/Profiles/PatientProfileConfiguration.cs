using Dawaya.Api.Domain.Profiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dawaya.Api.Data.Configurations.Profiles;

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

        // Named indexes: two unnamed HasIndex calls on the same column would collapse into one,
        // leaving only the filtered index and no index for listing a user's profiles.
        b.HasIndex(x => x.OwnerUserId, "IX_Profiles_OwnerUserId");
        // At most one "myself" profile per account
        b.HasIndex(x => x.OwnerUserId, "UX_Profiles_OneSelfPerUser").IsUnique().HasFilter("[IsSelf] = 1");
    }
}
