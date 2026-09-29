using Dawaya.Api.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dawaya.Api.Features.Catalog;

// Public catalog data (not health data): suggestions when adding an allergy.
[ApiController]
[AllowAnonymous]
[Route(ApiRoutes.V1 + "/ingredients")]
public sealed class IngredientsController(IIngredientSearch ingredients) : ControllerBase
{
    [HttpGet(Name = "SearchIngredients")]
    [ProducesResponseType<IReadOnlyList<IngredientResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<IngredientResponse>>> Search(
        [FromQuery] string? search, [FromQuery] int? limit, CancellationToken ct) =>
        Ok(await ingredients.SearchAsync(search, limit, ct));
}
