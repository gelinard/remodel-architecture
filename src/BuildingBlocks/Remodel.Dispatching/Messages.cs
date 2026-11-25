namespace Remodel.Dispatching;

/// <summary>A request that reads data and returns <typeparamref name="TResult"/>. A query must not change state.</summary>
public interface IQuery<TResult>
{
}

/// <summary>A request that changes state and returns <typeparamref name="TResult"/>.</summary>
public interface ICommand<TResult>
{
}

/// <summary>Handles one query type. Each query has exactly one handler.</summary>
public interface IQueryHandler<TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    Task<TResult> Handle(TQuery query, CancellationToken ct);
}

/// <summary>Handles one command type. Each command has exactly one handler.</summary>
public interface ICommandHandler<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    Task<TResult> Handle(TCommand command, CancellationToken ct);
}

/// <summary>
/// Wraps the handling of a query or command, for cross-cutting work such as logging or validation.
/// Call <c>next</c> to continue to the next behavior (or the handler). Don't call it to stop the pipeline early.
/// </summary>
/// <remarks>Behaviors run in the order they are registered: the first one registered is the outermost.</remarks>
public interface IPipelineBehavior<TMessage, TResult>
{
    Task<TResult> Handle(TMessage message, Func<Task<TResult>> next, CancellationToken ct);
}
