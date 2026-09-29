using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Dawaya.Api.Infrastructure.Security;

public interface ICurrentUser
{
    // The signed-in user's id. Only valid on endpoints that require authentication.
    string Id { get; }
}

public sealed class HttpCurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    public string Id =>
        accessor.HttpContext?.User.FindFirstValue(JwtRegisteredClaimNames.Sub)
        ?? throw new InvalidOperationException("No authenticated user on this request.");
}
