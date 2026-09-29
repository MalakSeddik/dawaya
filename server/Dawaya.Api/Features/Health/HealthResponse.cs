namespace Dawaya.Api.Features.Health;

public sealed record HealthResponse(HealthState Status, IReadOnlyDictionary<string, HealthState> Checks, DateTime CheckedAt);
