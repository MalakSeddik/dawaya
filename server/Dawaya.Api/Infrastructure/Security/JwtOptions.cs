using System.ComponentModel.DataAnnotations;

namespace Dawaya.Api.Infrastructure.Security;

// SigningKey is a secret: user-secrets in development, an environment variable in production
// (Jwt__SigningKey). It is never committed.
public sealed class JwtOptions
{
    public const string Section = "Jwt";

    [Required] public string Issuer { get; set; } = "dawaya-api";
    [Required] public string Audience { get; set; } = "dawaya-clients";

    [Required, MinLength(32, ErrorMessage = "Jwt:SigningKey must be at least 32 characters.")]
    public string SigningKey { get; set; } = string.Empty;

    [Range(1, 60)] public int AccessTokenMinutes { get; set; } = 15;
    [Range(1, 365)] public int RefreshTokenDays { get; set; } = 30;
}
