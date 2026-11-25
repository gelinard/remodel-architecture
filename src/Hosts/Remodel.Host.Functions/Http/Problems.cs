using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Remodel.Dispatching;

namespace Remodel.Host.Functions.Http;

/// <summary>
/// Every HTTP error response is built here, as problem details (RFC 9457).
/// Expected failures arrive as a <see cref="Result"/>; exceptions arrive through <see cref="ErrorHandlingMiddleware"/>.
/// </summary>
internal static class Problems
{
    /// <summary>204 No Content when the command worked, otherwise a problem response.</summary>
    public static IActionResult ToActionResult(this Result result) =>
        result.Error is { } error ? AsActionResult(FromError(error)) : new NoContentResult();

    public static ObjectResult AsActionResult(ProblemDetails problem) => new(problem) { StatusCode = problem.Status };

    /// <summary>404 for a query that found nothing.</summary>
    public static IActionResult NotFound(string detail) => AsActionResult(FromError(new Error(ErrorKind.NotFound, detail)));

    public static ProblemDetails FromError(Error error) => error.Kind switch
    {
        ErrorKind.NotFound => Problem(StatusCodes.Status404NotFound, "Not found", error.Message),
        ErrorKind.Conflict => Problem(StatusCodes.Status409Conflict, "Conflict", error.Message),
        ErrorKind.Rejected => Problem(StatusCodes.Status422UnprocessableEntity, "Rejected", error.Message),
        _ => throw new ArgumentOutOfRangeException(nameof(error), error.Kind, "Unknown error kind."),
    };

    public static ProblemDetails FromException(Exception exception) => exception switch
    {
        ValidationException validation => new ValidationProblemDetails(
            validation.Errors
                .GroupBy(e => e.Field)
                .ToDictionary(group => group.Key, group => group.Select(e => e.Message).ToArray()))
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "The request is not valid",
        },
        JsonException => Problem(StatusCodes.Status400BadRequest, "The request body is not valid JSON", detail: null),

        // No details for unexpected errors: they could show internals. The middleware logs the exception.
        _ => Problem(StatusCodes.Status500InternalServerError, "Something went wrong", detail: null),
    };

    private static ProblemDetails Problem(int status, string title, string? detail) =>
        new() { Status = status, Title = title, Detail = detail };
}
