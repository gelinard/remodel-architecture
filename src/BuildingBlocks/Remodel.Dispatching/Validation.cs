namespace Remodel.Dispatching;

/// <summary>
/// Checks a query or command before its handler runs. A message can have any number of validators.
/// They run in <see cref="ValidationBehavior{TMessage,TResult}"/>.
/// </summary>
public interface IValidator<TMessage>
{
    /// <summary>Returns one error per problem, or nothing when the message is valid.</summary>
    IEnumerable<ValidationError> Validate(TMessage message);
}

public sealed record ValidationError(string Field, string Message);

/// <summary>Thrown when a message is not valid. Hosts turn it into a "bad request" response.</summary>
public sealed class ValidationException(IReadOnlyList<ValidationError> errors)
    : Exception("Validation failed: " + string.Join("; ", errors.Select(error => $"{error.Field}: {error.Message}")))
{
    public IReadOnlyList<ValidationError> Errors { get; } = errors;
}

/// <summary>Runs every validator for the message and stops the pipeline with a <see cref="ValidationException"/> if any of them reports an error.</summary>
public sealed class ValidationBehavior<TMessage, TResult>(IEnumerable<IValidator<TMessage>> validators)
    : IPipelineBehavior<TMessage, TResult>
{
    public async Task<TResult> Handle(TMessage message, Func<Task<TResult>> next, CancellationToken ct)
    {
        var errors = validators.SelectMany(validator => validator.Validate(message)).ToList();
        if (errors.Count > 0)
        {
            throw new ValidationException(errors);
        }

        return await next();
    }
}
