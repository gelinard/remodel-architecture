using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;

namespace Remodel.Dispatching.Tests;

/// <summary>
/// A real service provider with the dispatcher and this assembly's handlers and validators,
/// resolved inside a scope like a host would. Logs go to an in-memory collector.
/// </summary>
internal sealed class TestHost : IDisposable
{
    private readonly ServiceProvider _root;
    private readonly IServiceScope _scope;

    public TestHost(Action<IServiceCollection>? configure = null)
    {
        var services = new ServiceCollection()
            .AddLogging(logging => logging.AddFakeLogging())
            .AddDispatcher()
            .AddHandlersFrom(typeof(TestHost).Assembly)
            .AddValidatorsFrom(typeof(TestHost).Assembly)
            .AddSingleton<Recorder>();

        configure?.Invoke(services);

        _root = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
        _scope = _root.CreateScope();
    }

    public IDispatcher Dispatcher => _scope.ServiceProvider.GetRequiredService<IDispatcher>();

    public Recorder Recorder => _scope.ServiceProvider.GetRequiredService<Recorder>();

    public FakeLogCollector Logs => _root.GetFakeLogCollector();

    public void Dispose()
    {
        _scope.Dispose();
        _root.Dispose();
    }
}
