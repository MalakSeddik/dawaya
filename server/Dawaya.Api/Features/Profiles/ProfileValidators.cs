using FluentValidation;

namespace Dawaya.Api.Features.Profiles;

public sealed class SaveProfileRequestValidator : AbstractValidator<SaveProfileRequest>
{
    public SaveProfileRequestValidator()
    {
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Relationship).MaximumLength(50);
        RuleFor(x => x.AgeGroup).IsInEnum();
    }
}

public sealed class AddAllergyRequestValidator : AbstractValidator<AddAllergyRequest>
{
    public AddAllergyRequestValidator()
    {
        RuleFor(x => x).Must(x => x.IngredientId is not null || !string.IsNullOrWhiteSpace(x.AllergyText))
            .WithName("AllergyText")
            .WithMessage("Choose an ingredient or type the allergy.");
        RuleFor(x => x.IngredientId).GreaterThan(0);
        RuleFor(x => x.AllergyText).MaximumLength(200);
    }
}
