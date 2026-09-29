using Dawaya.Api.Domain.Identity;
using Microsoft.AspNetCore.Identity;

namespace Dawaya.Api.Infrastructure.Security;

public sealed class PasswordNotEmailValidator : IPasswordValidator<ApplicationUser>
{
    public Task<IdentityResult> ValidateAsync(UserManager<ApplicationUser> manager, ApplicationUser user, string? password)
    {
        var same = password is not null && user.Email is not null
            && string.Equals(password.Trim(), user.Email.Trim(), StringComparison.OrdinalIgnoreCase);
        return Task.FromResult(same
            ? IdentityResult.Failed(new IdentityError { Code = "PasswordSameAsEmail", Description = "Password must not be the same as the email." })
            : IdentityResult.Success);
    }
}
