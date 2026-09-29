using Dawaya.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Dawaya.Api.Features.Catalog;

public sealed record IngredientResponse(int Id, string NameEn, string? NameAr);

public interface IIngredientSearch
{
    Task<IReadOnlyList<IngredientResponse>> SearchAsync(string? search, int? limit, CancellationToken ct);
}

// Simple contains-match for now. Arabic normalization (alef/yaa/taa marbuta, diacritics) comes in Slice 3.
public sealed class IngredientSearch(AppDbContext db) : IIngredientSearch
{
    public const int MinSearchLength = 2;
    public const int DefaultLimit = 10;
    public const int MaxLimit = 20;

    public async Task<IReadOnlyList<IngredientResponse>> SearchAsync(string? search, int? limit, CancellationToken ct)
    {
        var term = search?.Trim();
        if (term is null || term.Length < MinSearchLength) return [];

        var take = Math.Clamp(limit ?? DefaultLimit, 1, MaxLimit);
        var lowered = term.ToLowerInvariant();
        // Runs as SQL (LOWER/LIKE), so .NET culture rules do not apply.
#pragma warning disable CA1304, CA1311, CA1862
        return await db.ActiveIngredients.AsNoTracking()
            .Where(i => i.NameEn.ToLower().Contains(lowered) || (i.NameAr != null && i.NameAr.Contains(term)))
            .OrderBy(i => i.NameEn)
            .Take(take)
            .Select(i => new IngredientResponse(i.Id, i.NameEn, i.NameAr))
            .ToListAsync(ct);
#pragma warning restore CA1304, CA1311, CA1862
    }
}
