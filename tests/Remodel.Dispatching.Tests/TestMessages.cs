namespace Remodel.Dispatching.Tests;

// Small messages and handlers used by the tests. AddHandlersFrom finds the handlers by scanning this assembly.

public sealed record Ping(string Text) : IQuery<string>;

internal sealed class PingHandler : IQueryHandler<Ping, string>
{
    public Task<string> Handle(Ping query, CancellationToken ct) => Task.FromResult($"pong: {query.Text}");
}

public sealed record Rename(string NewName) : ICommand<int>;

internal sealed class RenameHandler : ICommandHandler<Rename, int>
{
    public Task<int> Handle(Rename command, CancellationToken ct) => Task.FromResult(command.NewName.Length);
}

// Two validators for the same command, to show that every validator runs.
internal sealed class RenameStartsWithCapitalValidator : IValidator<Rename>
{
    public IEnumerable<ValidationError> Validate(Rename message)
    {
        if (message.NewName.Length == 0 || !char.IsUpper(message.NewName[0]))
        {
            yield return new(nameof(Rename.NewName), "Must start with a capital letter.");
        }
    }
}

internal sealed class RenameMaxLengthValidator : IValidator<Rename>
{
    public IEnumerable<ValidationError> Validate(Rename message)
    {
        if (message.NewName.Length > 10)
        {
            yield return new(nameof(Rename.NewName), "Must be 10 characters or less.");
        }
    }
}

public sealed record Reject : ICommand<Result>;

internal sealed class RejectHandler : ICommandHandler<Reject, Result>
{
    public Task<Result> Handle(Reject command, CancellationToken ct) =>
        Task.FromResult(Result.Failure(ErrorKind.Conflict, "Already done."));
}

public sealed record Track : IQuery<string>;

internal sealed class TrackHandler(Recorder recorder) : IQueryHandler<Track, string>
{
    public Task<string> Handle(Track query, CancellationToken ct)
    {
        recorder.Steps.Add("handler");
        return Task.FromResult("done");
    }
}

public sealed record CaptureToken : IQuery<CancellationToken>;

internal sealed class CaptureTokenHandler : IQueryHandler<CaptureToken, CancellationToken>
{
    public Task<CancellationToken> Handle(CaptureToken query, CancellationToken ct) => Task.FromResult(ct);
}

public sealed record Explode : ICommand<int>;

internal sealed class ExplodeHandler : ICommandHandler<Explode, int>
{
    public Task<int> Handle(Explode command, CancellationToken ct) => throw new BoomException();
}

public sealed class BoomException : Exception;

/// <summary>Has no handler on purpose.</summary>
public sealed record Orphan : IQuery<int>;

internal sealed class Recorder
{
    public List<string> Steps { get; } = [];
}

internal sealed class OuterBehavior<TMessage, TResult>(Recorder recorder) : IPipelineBehavior<TMessage, TResult>
{
    public async Task<TResult> Handle(TMessage message, Func<Task<TResult>> next, CancellationToken ct)
    {
        recorder.Steps.Add("outer:before");
        var result = await next();
        recorder.Steps.Add("outer:after");
        return result;
    }
}

internal sealed class InnerBehavior<TMessage, TResult>(Recorder recorder) : IPipelineBehavior<TMessage, TResult>
{
    public async Task<TResult> Handle(TMessage message, Func<Task<TResult>> next, CancellationToken ct)
    {
        recorder.Steps.Add("inner:before");
        var result = await next();
        recorder.Steps.Add("inner:after");
        return result;
    }
}

internal sealed class CachedTrackBehavior : IPipelineBehavior<Track, string>
{
    public Task<string> Handle(Track message, Func<Task<string>> next, CancellationToken ct) => Task.FromResult("from cache");
}
