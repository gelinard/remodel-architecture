namespace Remodel.Dispatching;

/// <summary>The single entry point that hosts (HTTP, queue, ...) use to run queries and commands.</summary>
public interface IDispatcher
{
    Task<TResult> Query<TResult>(IQuery<TResult> query, CancellationToken ct);

    Task<TResult> Send<TResult>(ICommand<TResult> command, CancellationToken ct);
}
