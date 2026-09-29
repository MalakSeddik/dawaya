using Dawaya.Api.Infrastructure;
using Dawaya.Api.Infrastructure.Results;
using Microsoft.AspNetCore.Mvc;

namespace Dawaya.Api.Features.Profiles;

// BR-06: family profiles. Requires sign-in (fallback policy); ownership is enforced in ProfileService.
[ApiController]
[Route(ApiRoutes.V1 + "/profiles")]
public sealed class ProfilesController(IProfileService profiles) : ControllerBase
{
    [HttpGet(Name = "ListProfiles")]
    [ProducesResponseType<IReadOnlyList<ProfileResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProfileResponse>>> List(CancellationToken ct) =>
        Ok(await profiles.ListAsync(ct));

    [HttpGet("{id:int}", Name = "GetProfile")]
    [ProducesResponseType<ProfileResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<ActionResult<ProfileResponse>> Get(int id, CancellationToken ct) =>
        this.ToActionResult(await profiles.GetAsync(id, ct));

    [HttpPost(Name = "CreateProfile")]
    [ProducesResponseType<ProfileResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
    public async Task<ActionResult<ProfileResponse>> Create(SaveProfileRequest request, CancellationToken ct) =>
        this.ToActionResult(await profiles.CreateAsync(request, ct));

    [HttpPut("{id:int}", Name = "UpdateProfile")]
    [ProducesResponseType<ProfileResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<ActionResult<ProfileResponse>> Update(int id, SaveProfileRequest request, CancellationToken ct) =>
        this.ToActionResult(await profiles.UpdateAsync(id, request, ct));

    [HttpDelete("{id:int}", Name = "DeleteProfile")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct) =>
        this.ToNoContent(await profiles.DeleteAsync(id, ct));

    [HttpPost("{id:int}/allergies", Name = "AddAllergy")]
    [ProducesResponseType<ProfileResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
    public async Task<ActionResult<ProfileResponse>> AddAllergy(int id, AddAllergyRequest request, CancellationToken ct) =>
        this.ToActionResult(await profiles.AddAllergyAsync(id, request, ct));

    [HttpDelete("{id:int}/allergies/{allergyId:int}", Name = "RemoveAllergy")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<IActionResult> RemoveAllergy(int id, int allergyId, CancellationToken ct) =>
        this.ToNoContent(await profiles.RemoveAllergyAsync(id, allergyId, ct));
}
