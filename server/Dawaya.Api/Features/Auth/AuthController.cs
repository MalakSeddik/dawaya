using Dawaya.Api.Infrastructure;
using Dawaya.Api.Infrastructure.Results;
using Dawaya.Api.Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Dawaya.Api.Features.Auth;

[ApiController]
[AllowAnonymous]
[EnableRateLimiting(SecuritySetup.AuthRateLimitPolicy)]
[Route(ApiRoutes.V1 + "/auth")]
public sealed class AuthController(IAuthService auth) : ControllerBase
{
    [HttpPost("register", Name = "Register")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken ct) =>
        this.ToActionResult(await auth.RegisterAsync(request, ct));

    [HttpPost("login", Name = "Login")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken ct) =>
        this.ToActionResult(await auth.LoginAsync(request, ct));

    [HttpPost("refresh", Name = "RefreshToken")]
    [DisableRateLimiting] // Called silently every 15 minutes by every signed-in client.
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
    public async Task<ActionResult<AuthResponse>> Refresh(RefreshRequest request, CancellationToken ct) =>
        this.ToActionResult(await auth.RefreshAsync(request, ct));

    [HttpPost("logout", Name = "Logout")]
    [DisableRateLimiting]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(LogoutRequest request, CancellationToken ct)
    {
        await auth.LogoutAsync(request, ct);
        return NoContent();
    }

    [HttpPost("forgot-password", Name = "ForgotPassword")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request, CancellationToken ct)
    {
        await auth.ForgotPasswordAsync(request, ct);
        return Accepted();
    }

    [HttpPost("reset-password", Name = "ResetPassword")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request, CancellationToken ct) =>
        this.ToNoContent(await auth.ResetPasswordAsync(request, ct));
}
