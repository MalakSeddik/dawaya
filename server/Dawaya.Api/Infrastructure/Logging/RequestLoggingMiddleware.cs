using System.Diagnostics;

namespace Dawaya.Api.Infrastructure.Logging;

// Logs method, route *template* (e.g. "api/v1/profiles/{id}"), status and duration.
// Never the raw path, query string, headers or body: these can hold tokens or health data (BR-36).
public sealed partial class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var start = Stopwatch.GetTimestamp();
        try
        {
            await next(context);
        }
        finally
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                var route = (context.GetEndpoint() as RouteEndpoint)?.RoutePattern.RawText ?? "(unmatched)";
                var elapsedMs = Stopwatch.GetElapsedTime(start).TotalMilliseconds;
                LogRequest(logger, context.Request.Method, route, context.Response.StatusCode, elapsedMs);
            }
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "HTTP {Method} {Route} responded {StatusCode} in {ElapsedMs:0.0} ms")]
    private static partial void LogRequest(ILogger logger, string method, string route, int statusCode, double elapsedMs);
}
