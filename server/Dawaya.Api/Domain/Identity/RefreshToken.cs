namespace Dawaya.Api.Domain.Identity;

// Only the SHA-256 hash of a refresh token is stored; the raw token lives only on the client.
// Tokens rotate on every refresh. All tokens from one sign-in share a FamilyId, so reuse of an
// already-rotated token revokes the whole family (likely theft).
public class RefreshToken : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    public string TokenHash { get; set; } = string.Empty;
    public Guid FamilyId { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public int? ReplacedByTokenId { get; set; }
}
