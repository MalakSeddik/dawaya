using Dawaya.Api.Data;
using FluentValidation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Dawaya.Tests.Infrastructure;

// Boots the real API in memory. The database is swapped for EF InMemory so the plumbing tests
// run anywhere (including CI without SQL Server). Test-only controllers from this assembly are added.
public sealed class ApiFactory : WebApplicationFactory<Program>
{
    public CapturingLoggerProvider Logs { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureLogging(l => l.AddProvider(Logs));
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();
            var dbName = $"dawaya-tests-{Guid.NewGuid()}";
            services.AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName));

            services.AddControllers().AddApplicationPart(typeof(ApiFactory).Assembly);
            services.AddValidatorsFromAssemblyContaining<ApiFactory>();
        });
    }
}
