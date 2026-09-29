using Dawaya.Api.Infrastructure;
using Dawaya.Api.Infrastructure.Results;
using Microsoft.AspNetCore.Mvc;

namespace Dawaya.Api.Features.Account;

[ApiController]
[Route(ApiRoutes.V1 + "/me")]
public sealed class AccountController(IAccountService account) : ControllerBase
{
    [HttpGet(Name = "GetCurrentUser")]
    [ProducesResponseType<CurrentUserResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<CurrentUserResponse>> Get(CancellationToken ct) =>
        this.ToActionResult(await account.GetCurrentAsync(ct));

    [HttpPut("preferences", Name = "UpdatePreferences")]
    [ProducesResponseType<CurrentUserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    public async Task<ActionResult<CurrentUserResponse>> UpdatePreferences(UpdatePreferencesRequest request, CancellationToken ct) =>
        this.ToActionResult(await account.UpdatePreferencesAsync(request, ct));

    // BR-38: download everything. Health data must not be cached by browsers or proxies.
    [HttpGet("export", Name = "ExportAccount")]
    [ProducesResponseType<AccountExport>(StatusCodes.Status200OK)]
    public async Task<ActionResult<AccountExport>> Export(CancellationToken ct)
    {
        var result = await account.ExportAsync(ct);
        if (!result.IsSuccess) return this.ToProblem(result.Error);

        Response.Headers.CacheControl = "no-store";
        Response.Headers.ContentDisposition = $"attachment; filename=\"dawaya-export-{result.Value.ExportedAt:yyyy-MM-dd}.json\"";
        return Ok(result.Value);
    }

    // BR-38: permanently delete the account and everything in it. POST because it carries the password.
    [HttpPost("delete", Name = "DeleteAccount")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    public async Task<IActionResult> Delete(DeleteAccountRequest request, CancellationToken ct) =>
        this.ToNoContent(await account.DeleteAsync(request, ct));
}
