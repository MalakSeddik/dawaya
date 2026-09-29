using System.Text.Json.Serialization;
using Dawaya.Api.Data;
using Dawaya.Api.Features.Account;
using Dawaya.Api.Features.Auth;
using Dawaya.Api.Features.Catalog;
using Dawaya.Api.Features.Health;
using Dawaya.Api.Features.Profiles;
using Dawaya.Api.Infrastructure;
using Dawaya.Api.Infrastructure.Email;
using Dawaya.Api.Infrastructure.ErrorHandling;
using Dawaya.Api.Infrastructure.Logging;
using Dawaya.Api.Infrastructure.OpenApi;
using Dawaya.Api.Infrastructure.Security;
using Dawaya.Api.Infrastructure.Storage;
using Dawaya.Api.Infrastructure.Validation;
using FluentValidation;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

if (!builder.Environment.IsDevelopment())
    builder.Logging.ClearProviders().AddJsonConsole();

builder.Services.AddSingleton(TimeProvider.System);

// Never call EnableSensitiveDataLogging: it would write health data into logs (BR-36).
builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddDawayaSecurity(builder.Configuration);
builder.Services.AddDawayaEmail(builder.Configuration, builder.Environment);
builder.Services.AddSingleton<IBlobStorage>(new LocalDiskBlobStorage(
    Path.GetFullPath(builder.Configuration["Storage:LocalRoot"] ?? "App_Data/blobs", builder.Environment.ContentRootPath)));

builder.Services
    .AddControllers(o =>
    {
        o.Filters.Add<FluentValidationFilter>();
        // JSON only: keeps the OpenAPI contract (and the generated client) free of text/plain variants.
        o.OutputFormatters.RemoveType<StringOutputFormatter>();
        foreach (var json in o.OutputFormatters.OfType<SystemTextJsonOutputFormatter>())
            json.SupportedMediaTypes.Remove("text/json");
    })
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
// The OpenAPI generator reads these options, so enums appear as strings in the contract too.
builder.Services.ConfigureHttpJsonOptions(o => o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddDawayaProblemDetails();
builder.Services.AddDawayaOpenApi();
builder.Services.AddDawayaCors(builder.Configuration);

builder.Services.AddHealthChecks().AddDbContextCheck<AppDbContext>("database");
builder.Services.AddScoped<IHealthReporter, HealthReporter>();

builder.Services.AddScoped<IAccessTokenIssuer, AccessTokenIssuer>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IIngredientSearch, IngredientSearch>();
if (!BuildContext.IsGeneratingOpenApi) builder.Services.AddHostedService<RefreshTokenCleanupService>();

var app = builder.Build();

app.UseDawayaProblemDetails();
app.UseMiddleware<RequestLoggingMiddleware>();
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}
app.UseCors(CorsSetup.PolicyName);
app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();

app.UseDawayaOpenApi();
app.MapControllers();

await app.RunAsync();

// Exposed for WebApplicationFactory in Dawaya.Tests.
public partial class Program;
