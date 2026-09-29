using Microsoft.AspNetCore.Http.Features;

namespace Dawaya.Api.Infrastructure.ErrorHandling;

// RFC 7807 ProblemDetails for every error, including unhandled exceptions and bare status codes.
public static class ProblemDetailsSetup
{
    public static IServiceCollection AddDawayaProblemDetails(this IServiceCollection services)
    {
        services.AddProblemDetails(o => o.CustomizeProblemDetails = ctx =>
        {
            var activity = ctx.HttpContext.Features.Get<IHttpActivityFeature>()?.Activity;
            ctx.ProblemDetails.Extensions["traceId"] = activity?.Id ?? ctx.HttpContext.TraceIdentifier;
        });
        services.AddExceptionHandler<UnhandledExceptionHandler>();
        return services;
    }

    public static WebApplication UseDawayaProblemDetails(this WebApplication app)
    {
        app.UseExceptionHandler();
        app.UseStatusCodePages();
        return app;
    }
}
