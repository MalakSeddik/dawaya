using Dawaya.Api.Data;
using FluentValidation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;

namespace Dawaya.Tests.Infrastructure;

// Boots the real API in memory against a private in-memory SQLite database (relational: foreign keys,
// cascades and bulk updates behave like SQL Server), so tests run anywhere including CI.
// Test-only controllers from this assembly are added.
public class ApiFactory : WebApplicationFactory<Program>
{
    public const string TestSigningKey = "test-signing-key-for-dawaya-tests-only-0123456789";

    private readonly SqliteConnection _connection = new("DataSource=:memory:");
    private readonly string _tempRoot = Path.Combine(Path.GetTempPath(), "dawaya-tests", Guid.NewGuid().ToString("N"));

    public ApiFactory() => _connection.Open();

    public CapturingLoggerProvider Logs { get; } = new();
    // Starts at the real time (JWT validation uses the real clock); tests advance it for expiry cases.
    public FakeTimeProvider Clock { get; } = new(DateTimeOffset.UtcNow);
    public string EmailDirectory => Path.Combine(_tempRoot, "emails");
    public string BlobRoot => Path.Combine(_tempRoot, "blobs");

    // Very high by default so tests are not throttled; the rate-limit test lowers it.
    protected virtual int AuthPermitLimit => 100_000;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("Jwt:SigningKey", TestSigningKey);
        builder.UseSetting("RateLimiting:Auth:PermitLimit", AuthPermitLimit.ToString(System.Globalization.CultureInfo.InvariantCulture));
        builder.UseSetting("Email:PickupDirectory", EmailDirectory);
        builder.UseSetting("Storage:LocalRoot", BlobRoot);
        builder.UseSetting("App:WebBaseUrl", "https://app.test");
        builder.ConfigureLogging(l => l.AddProvider(Logs));
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();
            services.AddDbContext<AppDbContext>(o => o.UseSqlite(_connection));

            services.RemoveAll<TimeProvider>();
            services.AddSingleton<TimeProvider>(Clock);

            services.AddControllers().AddApplicationPart(typeof(ApiFactory).Assembly);
            services.AddValidatorsFromAssemblyContaining<ApiFactory>();
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);
        using var scope = host.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();
        return host;
    }

    public async Task<T> WithDbAsync<T>(Func<AppDbContext, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();
        return await action(scope.ServiceProvider.GetRequiredService<AppDbContext>());
    }

    public Task WithDbAsync(Func<AppDbContext, Task> action) =>
        WithDbAsync(async db => { await action(db); return true; });

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing) return;
        _connection.Dispose();
        if (Directory.Exists(_tempRoot)) Directory.Delete(_tempRoot, recursive: true);
    }
}
