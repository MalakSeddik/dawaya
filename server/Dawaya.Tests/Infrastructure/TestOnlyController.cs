using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Dawaya.Tests.Infrastructure;

// Endpoints that exist only inside the test host, to exercise error handling and validation.
[ApiController]
[Route("api/v1/_test")]
public sealed class TestOnlyController : ControllerBase
{
    public const string SecretMessage = "patient-allergy-penicillin-secret";

    [HttpGet("throw")]
    public IActionResult Throw() => throw new InvalidOperationException(SecretMessage);

    [HttpPost("validate")]
    public IActionResult Validate(SampleRequest request) => Ok(request);
}

public sealed record SampleRequest(string? DisplayName, int Quantity);

public sealed class SampleRequestValidator : AbstractValidator<SampleRequest>
{
    public SampleRequestValidator()
    {
        RuleFor(x => x.DisplayName).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}
