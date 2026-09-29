using System.Reflection;

namespace Dawaya.Api.Infrastructure;

public static class BuildContext
{
    // True when the build-time OpenAPI generator (dotnet-getdocument) is starting the app only to read
    // its endpoints. Secrets are not available then, so startup checks and background jobs are skipped.
    public static bool IsGeneratingOpenApi { get; } =
        Assembly.GetEntryAssembly()?.GetName().Name == "GetDocument.Insider";
}
