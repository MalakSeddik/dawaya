using Dawaya.Api.Features.Auth;
using FluentValidation;

namespace Dawaya.Api.Features.Account;

public sealed class UpdatePreferencesRequestValidator : AbstractValidator<UpdatePreferencesRequest>
{
    public UpdatePreferencesRequestValidator() =>
        RuleFor(x => x.PreferredLanguage).Must(l => AuthRules.Languages.Contains(l))
            .WithMessage("PreferredLanguage must be 'ar' or 'en'.");
}

public sealed class DeleteAccountRequestValidator : AbstractValidator<DeleteAccountRequest>
{
    public DeleteAccountRequestValidator() =>
        RuleFor(x => x.Password).NotEmpty().MaximumLength(AuthRules.MaxPasswordLength);
}
