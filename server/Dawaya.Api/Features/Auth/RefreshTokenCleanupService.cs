namespace Dawaya.Api.Features.Auth;

// Background job (CLAUDE.md rule 7): removes long-expired and revoked refresh tokens once a day.
public sealed partial class RefreshTokenCleanupService(
    IServiceScopeFactory scopes,
    ILogger<RefreshTokenCleanupService> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(24);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval);
        do
        {
            try
            {
                await using var scope = scopes.CreateAsyncScope();
                var tokens = scope.ServiceProvider.GetRequiredService<IRefreshTokenService>();
                var removed = await tokens.DeleteStaleAsync(stoppingToken);
                LogCleanup(logger, removed);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                LogFailed(logger, ex.GetType().Name);
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Removed {Count} stale refresh tokens")]
    private static partial void LogCleanup(ILogger logger, int count);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Refresh token cleanup failed: {ExceptionType}")]
    private static partial void LogFailed(ILogger logger, string exceptionType);
}
