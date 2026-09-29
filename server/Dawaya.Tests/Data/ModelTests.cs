using Dawaya.Api.Data;
using Dawaya.Api.Domain.Profiles;
using Microsoft.EntityFrameworkCore;

namespace Dawaya.Tests.Data;

public sealed class ModelTests
{
    // Uses the SQL Server provider without opening a connection.
    private static AppDbContext CreateSqlServerContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer("Server=unused;Database=unused;Trusted_Connection=True")
            .Options);

    [Fact]
    public void Migrations_are_in_sync_with_the_model()
    {
        using var db = CreateSqlServerContext();

        Assert.False(db.Database.HasPendingModelChanges(),
            "The EF model changed without a migration. Run: dotnet ef migrations add <Name> --project server/Dawaya.Api");
    }

    // BR-38: deleting a user must remove their rows (Cascade) or strip the reference (SetNull, for audit
    // and report rows). Any new table pointing at users must make that choice explicitly.
    private static readonly string[] PendingUserReferences =
    [
        // Admin who verified content. Deleting an admin account is decided in Slice 2 (audit/admin slice).
        "MedicineContent.VerifiedById",
    ];

    [Fact]
    public void Every_reference_to_a_user_is_removed_or_stripped_when_the_user_is_deleted()
    {
        using var db = CreateSqlServerContext();
        var userType = db.Model.FindEntityType(typeof(Dawaya.Api.Domain.Identity.ApplicationUser))!;

        var offenders = db.Model.GetEntityTypes()
            .SelectMany(e => e.GetForeignKeys())
            .Where(fk => fk.PrincipalEntityType == userType)
            .Where(fk => fk.DeleteBehavior is not (DeleteBehavior.Cascade or DeleteBehavior.SetNull or DeleteBehavior.ClientSetNull))
            .Select(fk => $"{fk.DeclaringEntityType.ClrType.Name}.{fk.Properties[0].Name}")
            .Except(PendingUserReferences)
            .ToList();

        Assert.Empty(offenders);
    }

    [Fact]
    public void Profiles_have_a_plain_owner_index_and_a_one_self_profile_per_user_index()
    {
        using var db = CreateSqlServerContext();
        var indexes = db.Model.FindEntityType(typeof(PatientProfile))!.GetIndexes().ToList();

        var lookup = Assert.Single(indexes, i => i.GetDatabaseName() == "IX_Profiles_OwnerUserId");
        Assert.False(lookup.IsUnique);
        Assert.Null(lookup.GetFilter());

        var oneSelf = Assert.Single(indexes, i => i.GetDatabaseName() == "UX_Profiles_OneSelfPerUser");
        Assert.True(oneSelf.IsUnique);
        Assert.Equal("[IsSelf] = 1", oneSelf.GetFilter());
    }
}
