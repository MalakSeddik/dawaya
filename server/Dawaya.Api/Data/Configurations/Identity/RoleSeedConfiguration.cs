using Dawaya.Api.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dawaya.Api.Data.Configurations.Identity;

// Seeds the three roles with fixed ids and stamps so the migration is deterministic.
public class RoleSeedConfiguration : IEntityTypeConfiguration<IdentityRole>
{
    public void Configure(EntityTypeBuilder<IdentityRole> b)
    {
        b.HasData(
            Role("5b0a1c8e-1f0e-4b7a-9d0c-3a1d2c4e5f01", Roles.User),
            Role("5b0a1c8e-1f0e-4b7a-9d0c-3a1d2c4e5f02", Roles.Admin),
            Role("5b0a1c8e-1f0e-4b7a-9d0c-3a1d2c4e5f03", Roles.Pharmacist));
    }

    private static IdentityRole Role(string id, string name) => new()
    {
        Id = id,
        Name = name,
        NormalizedName = name.ToUpperInvariant(),
        ConcurrencyStamp = id,
    };
}
