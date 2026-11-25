using Microsoft.Extensions.DependencyInjection;

namespace Remodel.Fleet.ContractTests;

/// <summary>
/// Base class for contract tests. It registers one adapter the same way a host does
/// (through its <c>AddFleet...</c> method) and resolves ports from a DI scope,
/// so the adapter's registration is tested too.
/// </summary>
public abstract class AdapterContract : IAsyncDisposable
{
    private readonly ServiceProvider _provider;
    private readonly AsyncServiceScope _scope;

    protected AdapterContract(LegacyFleetDatabase database, Action<IServiceCollection, string> addAdapter)
    {
        Database = database;

        var services = new ServiceCollection();
        addAdapter(services, database.ConnectionString);
        _provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
        _scope = _provider.CreateAsyncScope();
    }

    protected LegacyFleetDatabase Database { get; }

    protected static CancellationToken Ct => TestContext.Current.CancellationToken;

    protected TPort Port<TPort>() where TPort : notnull => _scope.ServiceProvider.GetRequiredService<TPort>();

    public async ValueTask DisposeAsync()
    {
        await _scope.DisposeAsync();
        await _provider.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}
