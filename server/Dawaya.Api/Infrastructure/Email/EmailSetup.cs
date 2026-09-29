namespace Dawaya.Api.Infrastructure.Email;

public static class EmailSetup
{
    public static IServiceCollection AddDawayaEmail(this IServiceCollection services, IConfiguration config, IHostEnvironment env)
    {
        var pickup = config["Email:PickupDirectory"];
        if (!string.IsNullOrWhiteSpace(pickup))
        {
            var directory = Path.GetFullPath(pickup, env.ContentRootPath);
            services.AddSingleton<IEmailSender>(sp => new FileEmailSender(directory, sp.GetRequiredService<TimeProvider>()));
        }
        else
        {
            services.AddSingleton<IEmailSender, UnconfiguredEmailSender>();
        }
        return services;
    }
}
