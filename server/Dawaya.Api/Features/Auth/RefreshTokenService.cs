using Dawaya.Api.Data;
using Dawaya.Api.Domain.Identity;
using Dawaya.Api.Infrastructure.Results;
using Dawaya.Api.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Dawaya.Api.Features.Auth;

public sealed record IssuedRefreshToken(string RawToken, DateTime ExpiresAt);

public sealed record RotatedRefreshToken(string UserId, IssuedRefreshToken Token);

public interface IRefreshTokenService
{
    Task<IssuedRefreshToken> IssueAsync(string userId, Guid? familyId, CancellationToken ct);
    Task<Result<RotatedRefreshToken>> RotateAsync(string rawToken, CancellationToken ct);
    Task RevokeFamilyAsync(string rawToken, CancellationToken ct);
    Task RevokeAllForUserAsync(string userId, CancellationToken ct);
    Task<int> DeleteStaleAsync(CancellationToken ct);
}

public sealed class RefreshTokenService(AppDbContext db, IOptions<JwtOptions> options, TimeProvider clock) : IRefreshTokenService
{
    public static readonly AppError InvalidToken =
        AppError.Unauthorized("invalid_refresh_token", "The session has expired. Please sign in again.");

    // Revoked or expired tokens are kept this long for reuse detection, then removed by the cleanup job.
    public static readonly TimeSpan StaleRetention = TimeSpan.FromDays(7);

    private DateTime Now => clock.GetUtcNow().UtcDateTime;

    public async Task<IssuedRefreshToken> IssueAsync(string userId, Guid? familyId, CancellationToken ct)
    {
        var (entity, raw) = NewToken(userId, familyId ?? Guid.NewGuid());
        db.RefreshTokens.Add(entity);
        await db.SaveChangesAsync(ct);
        return new IssuedRefreshToken(raw, entity.ExpiresAt);
    }

    public async Task<Result<RotatedRefreshToken>> RotateAsync(string rawToken, CancellationToken ct)
    {
        var hash = TokenHashing.Sha256Hex(rawToken);
        var current = await db.RefreshTokens.AsNoTracking().SingleOrDefaultAsync(t => t.TokenHash == hash, ct);
        if (current is null) return InvalidToken;
        if (current.RevokedAt is null && current.ExpiresAt <= Now) return InvalidToken;

        // Atomically claim the token. If it was already used (or another request just used it),
        // someone may have stolen it: end the whole sign-in.
        var now = Now;
        var claimed = await db.RefreshTokens
            .Where(t => t.Id == current.Id && t.RevokedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.RevokedAt, now), ct);
        if (claimed == 0)
        {
            await RevokeFamilyByIdAsync(current.FamilyId, ct);
            return InvalidToken;
        }

        var (next, raw) = NewToken(current.UserId, current.FamilyId);
        db.RefreshTokens.Add(next);
        await db.SaveChangesAsync(ct);
        await db.RefreshTokens.Where(t => t.Id == current.Id)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.ReplacedByTokenId, next.Id), ct);

        return new RotatedRefreshToken(current.UserId, new IssuedRefreshToken(raw, next.ExpiresAt));
    }

    public async Task RevokeFamilyAsync(string rawToken, CancellationToken ct)
    {
        var hash = TokenHashing.Sha256Hex(rawToken);
        var familyId = await db.RefreshTokens.Where(t => t.TokenHash == hash).Select(t => (Guid?)t.FamilyId).SingleOrDefaultAsync(ct);
        if (familyId is not null) await RevokeFamilyByIdAsync(familyId.Value, ct);
    }

    public Task RevokeAllForUserAsync(string userId, CancellationToken ct) =>
        db.RefreshTokens.Where(t => t.UserId == userId && t.RevokedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.RevokedAt, Now), ct);

    public Task<int> DeleteStaleAsync(CancellationToken ct)
    {
        var cutoff = Now - StaleRetention;
        return db.RefreshTokens
            .Where(t => t.ExpiresAt < cutoff || (t.RevokedAt != null && t.RevokedAt < cutoff))
            .ExecuteDeleteAsync(ct);
    }

    private Task<int> RevokeFamilyByIdAsync(Guid familyId, CancellationToken ct) =>
        db.RefreshTokens.Where(t => t.FamilyId == familyId && t.RevokedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.RevokedAt, Now), ct);

    private (RefreshToken Entity, string Raw) NewToken(string userId, Guid familyId)
    {
        var raw = TokenHashing.NewRawToken();
        var entity = new RefreshToken
        {
            UserId = userId,
            FamilyId = familyId,
            TokenHash = TokenHashing.Sha256Hex(raw),
            ExpiresAt = Now.AddDays(options.Value.RefreshTokenDays),
        };
        return (entity, raw);
    }
}
