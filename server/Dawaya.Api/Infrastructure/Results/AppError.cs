namespace Dawaya.Api.Infrastructure.Results;

// A business error returned by a service. `Code` is stable and machine-readable; clients map it to
// a translated message, so no user-facing text is decided on the server.
public sealed record AppError(string Code, int Status, string Title)
{
    public static AppError NotFound(string code = "not_found") =>
        new(code, StatusCodes.Status404NotFound, "The requested resource was not found.");

    public static AppError Conflict(string code, string title) => new(code, StatusCodes.Status409Conflict, title);

    public static AppError BadRequest(string code, string title) => new(code, StatusCodes.Status400BadRequest, title);

    public static AppError Unauthorized(string code, string title) => new(code, StatusCodes.Status401Unauthorized, title);
}
