using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Dawaya.Api.Infrastructure.Results;

// Turns an AppError into an RFC 7807 response with a machine-readable "code" extension.
public static class ProblemResults
{
    public const string CodeExtension = "code";

    public static ObjectResult ToProblem(this ControllerBase controller, AppError error)
    {
        var factory = controller.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();
        var problem = factory.CreateProblemDetails(controller.HttpContext, error.Status, error.Title);
        problem.Extensions[CodeExtension] = error.Code;
        return new ObjectResult(problem) { StatusCode = error.Status, ContentTypes = { "application/problem+json" } };
    }

    public static ActionResult<T> ToActionResult<T>(this ControllerBase controller, Result<T> result) =>
        result.IsSuccess ? controller.Ok(result.Value) : controller.ToProblem(result.Error);

    public static IActionResult ToNoContent(this ControllerBase controller, Result result) =>
        result.IsSuccess ? controller.NoContent() : controller.ToProblem(result.Error);
}
