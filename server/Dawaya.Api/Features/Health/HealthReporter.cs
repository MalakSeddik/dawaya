using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Dawaya.Api.Features.Health;

public interface IHealthReporter
{
    Task<HealthResponse> GetAsync(CancellationToken ct);
}

// Reports only check names and states, never exception messages or connection details.
public sealed class HealthReporter(HealthCheckService healthChecks, TimeProvider clock) : IHealthReporter
{
    public async Task<HealthResponse> GetAsync(CancellationToken ct)
    {
        var report = await healthChecks.CheckHealthAsync(ct);
        return new HealthResponse(
            Map(report.Status),
            report.Entries.ToDictionary(e => e.Key, e => Map(e.Value.Status)),
            clock.GetUtcNow().UtcDateTime);
    }

    private static HealthState Map(HealthStatus s) => s switch
    {
        HealthStatus.Healthy => HealthState.Healthy,
        HealthStatus.Degraded => HealthState.Degraded,
        _ => HealthState.Unhealthy,
    };
}
