using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Dawaya.Api.Infrastructure.ErrorHandling;

// Returns a generic 500. The exception message is never sent to the client or logged,
// because it could contain health data (BR-36).
public sealed partial class UnhandledExceptionHandler(
    IProblemDetailsService problemDetails,
    ILogger<UnhandledExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        LogUnhandled(logger, exception.GetType().FullName ?? "unknown", httpContext.GetEndpoint()?.DisplayName ?? "unknown");

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An unexpected error occurred.",
                Type = "https://tools.ietf.org/html/rfc9110#section-15.6.1",
            },
        });
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Unhandled exception {ExceptionType} in {Endpoint}")]
    private static partial void LogUnhandled(ILogger logger, string exceptionType, string endpoint);
}
