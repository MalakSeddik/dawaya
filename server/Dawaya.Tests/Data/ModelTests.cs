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
