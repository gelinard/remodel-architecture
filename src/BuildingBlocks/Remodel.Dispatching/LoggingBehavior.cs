using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Remodel.Dispatching;

/// <summary>
/// Logs every query and command: its name, how long it took, and the error when the result is an expected failure.
/// </summary>
/// <remarks>
/// The message's content is not logged, because it can contain personal data.
/// Exceptions are not caught here. They go up to the host, which logs them once.
/// </remarks>
public sealed class LoggingBehavior<TMessage, TResult>(ILogger<LoggingBehavior<TMessage, TResult>> logger)
    : IPipelineBehavior<TMessage, TResult>
{
    public async Task<TResult> Handle(TMessage message, Func<Task<TResult>> next, CancellationToken ct)
    {
        var started = Stopwatch.GetTimestamp();
        var result = await next();
        var elapsedMs = Stopwatch.GetElapsedTime(started).TotalMilliseconds;

        if (result is Result { Error: { } error })
        {
            logger.LogWarning("{MessageName} failed after {ElapsedMs:0} ms: {ErrorKind} {ErrorMessage}",
                typeof(TMessage).Name, elapsedMs, error.Kind, error.Message);
        }
        else
        {
            logger.LogInformation("{MessageName} handled in {ElapsedMs:0} ms", typeof(TMessage).Name, elapsedMs);
        }

        return result;
    }
}
