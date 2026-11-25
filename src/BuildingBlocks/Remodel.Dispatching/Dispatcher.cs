using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;

namespace Remodel.Dispatching;

/// <summary>
/// Finds the handler for a message, wraps it in the registered pipeline behaviors and runs it.
/// </summary>
/// <remarks>
/// The dispatcher only knows the message as <c>IQuery&lt;TResult&gt;</c>, but the handler is registered for the
/// concrete message type. Reflection bridges that gap once per message type by creating a typed runner,
/// which is then cached. Every later call is a normal generic call: no reflection, and exceptions from
/// handlers are not wrapped.
/// </remarks>
internal sealed class Dispatcher(IServiceProvider services) : IDispatcher
{
    public Task<TResult> Query<TResult>(IQuery<TResult> query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);
        return RunnerCache<TResult>.ForQuery(query.GetType()).Run(query, services, ct);
    }

    public Task<TResult> Send<TResult>(ICommand<TResult> command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);
        return RunnerCache<TResult>.ForCommand(command.GetType()).Run(command, services, ct);
    }

    private static Task<TResult> RunPipeline<TMessage, TResult>(
        TMessage message, IServiceProvider services, Func<Task<TResult>> handler, CancellationToken ct)
    {
        var behaviors = services.GetServices<IPipelineBehavior<TMessage, TResult>>().ToArray();

        // Build the chain from the inside out, so the first registered behavior ends up outermost.
        var next = handler;
        for (var i = behaviors.Length - 1; i >= 0; i--)
        {
            var behavior = behaviors[i];
            var inner = next;
            next = () => behavior.Handle(message, inner, ct);
        }

        return next();
    }

    private static InvalidOperationException NoHandlerFor(Type messageType) =>
        new($"No handler is registered for {messageType.Name}. " +
            "Register the module's handlers with services.AddHandlersFrom(assembly).");

    private static class RunnerCache<TResult>
    {
        private static readonly ConcurrentDictionary<Type, Runner<TResult>> Queries = new();
        private static readonly ConcurrentDictionary<Type, Runner<TResult>> Commands = new();

        public static Runner<TResult> ForQuery(Type queryType) =>
            Queries.GetOrAdd(queryType, static type => Create(typeof(QueryRunner<,>), type));

        public static Runner<TResult> ForCommand(Type commandType) =>
            Commands.GetOrAdd(commandType, static type => Create(typeof(CommandRunner<,>), type));

        private static Runner<TResult> Create(Type openRunnerType, Type messageType) =>
            (Runner<TResult>)Activator.CreateInstance(openRunnerType.MakeGenericType(messageType, typeof(TResult)))!;
    }

    private abstract class Runner<TResult>
    {
        public abstract Task<TResult> Run(object message, IServiceProvider services, CancellationToken ct);
    }

    private sealed class QueryRunner<TQuery, TResult> : Runner<TResult>
        where TQuery : IQuery<TResult>
    {
        public override Task<TResult> Run(object message, IServiceProvider services, CancellationToken ct)
        {
            var query = (TQuery)message;
            var handler = services.GetService<IQueryHandler<TQuery, TResult>>() ?? throw NoHandlerFor(typeof(TQuery));
            return RunPipeline(query, services, () => handler.Handle(query, ct), ct);
        }
    }

    private sealed class CommandRunner<TCommand, TResult> : Runner<TResult>
        where TCommand : ICommand<TResult>
    {
        public override Task<TResult> Run(object message, IServiceProvider services, CancellationToken ct)
        {
            var command = (TCommand)message;
            var handler = services.GetService<ICommandHandler<TCommand, TResult>>() ?? throw NoHandlerFor(typeof(TCommand));
            return RunPipeline(command, services, () => handler.Handle(command, ct), ct);
        }
    }
}
