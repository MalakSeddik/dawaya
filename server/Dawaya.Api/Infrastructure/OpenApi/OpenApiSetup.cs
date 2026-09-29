namespace Dawaya.Api.Infrastructure.OpenApi;

// The OpenAPI document is the contract (CLAUDE.md rule 4). packages/api-client is generated from it.
public static class OpenApiSetup
{
    public const string DocumentName = "v1";

    public static IServiceCollection AddDawayaOpenApi(this IServiceCollection services)
    {
        services.AddOpenApi(DocumentName, o => o.AddDocumentTransformer((doc, _, _) =>
        {
            doc.Info.Title = "Dawaya API";
            doc.Info.Version = "v1";
            doc.Info.Description = "Medication companion API. All dates are UTC (ISO 8601). Errors are RFC 7807 ProblemDetails.";
            return Task.CompletedTask;
        }));
        return services;
    }

    public static WebApplication UseDawayaOpenApi(this WebApplication app)
    {
        app.MapOpenApi();
        if (app.Environment.IsDevelopment())
        {
            app.UseSwaggerUI(o =>
            {
                o.SwaggerEndpoint($"/openapi/{DocumentName}.json", "Dawaya API v1");
                o.RoutePrefix = "swagger";
            });
        }
        return app;
    }
}
