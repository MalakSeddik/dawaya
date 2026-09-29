using Dawaya.Api.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace Dawaya.Api.Features.Health;

[ApiController]
[Route(ApiRoutes.V1 + "/health")]
public sealed class HealthController(IHealthReporter reporter) : ControllerBase
{
    [HttpGet(Name = "GetHealth")]
    [ProducesResponseType<HealthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<HealthResponse>(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<HealthResponse>> Get(CancellationToken ct)
    {
        var health = await reporter.GetAsync(ct);
        return health.Status == HealthState.Unhealthy
            ? StatusCode(StatusCodes.Status503ServiceUnavailable, health)
            : Ok(health);
    }
}
