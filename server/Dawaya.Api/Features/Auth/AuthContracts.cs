using Dawaya.Api.Features.Account;

namespace Dawaya.Api.Features.Auth;

public sealed record RegisterRequest(string Email, string Password, string DisplayName, string? PreferredLanguage);

public sealed record LoginRequest(string Email, string Password);

public sealed record RefreshRequest(string RefreshToken);

public sealed record LogoutRequest(string RefreshToken);

public sealed record ForgotPasswordRequest(string Email);

// The reset link carries the user id and token in the URL fragment, so they never reach server logs.
public sealed record ResetPasswordRequest(string UserId, string Token, string NewPassword);

public sealed record AuthResponse(
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt,
    CurrentUserResponse User);
