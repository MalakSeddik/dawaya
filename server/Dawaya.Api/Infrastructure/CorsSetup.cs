namespace Dawaya.Api.Infrastructure;

// Auth uses bearer tokens in headers (not cookies), so credentials are not allowed cross-origin.
public static class CorsSetup
{
    public const string PolicyName = "DawayaClients";

    public static IServiceCollection AddDawayaCors(this IServiceCollection services, IConfiguration config)
    {
        var origins = config.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        services.AddCors(o => o.AddPolicy(PolicyName, p => p
            .WithOrigins(origins)
            .AllowAnyHeader()
            .AllowAnyMethod()));
        return services;
    }
}
