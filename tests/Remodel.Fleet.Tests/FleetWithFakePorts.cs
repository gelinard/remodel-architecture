using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Remodel.Dispatching;
using Remodel.Fleet.Core;
using Remodel.Fleet.Core.Features.GetAvailableCars;
using Remodel.Fleet.Core.Features.GetCarHistory;
using Remodel.Fleet.Core.Features.ReturnCar;

namespace Remodel.Fleet.Tests;

/// <summary>
/// The Fleet module with fake ports: the real handlers, validators and pipeline, but no database.
/// Tests send messages through the dispatcher, exactly like a host does, so handlers can stay internal.
/// </summary>
internal sealed class FleetWithFakePorts : IDisposable
{
    private readonly ServiceProvider _root;
    private readonly IServiceScope _scope;

    public FleetWithFakePorts()
    {
        var services = new ServiceCollection()
            .AddDispatcher()
            .AddPipelineBehavior(typeof(ValidationBehavior<,>))
            .AddFleetCore()
            .AddSingleton(CarHistory)
            .AddSingleton(AvailableCars)
            .AddSingleton(CarReturns);

        _root = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
        _scope = _root.CreateScope();
    }

    public ICarHistoryReader CarHistory { get; } = Substitute.For<ICarHistoryReader>();

    public IAvailableCarsReader AvailableCars { get; } = Substitute.For<IAvailableCarsReader>();

    public ICarReturnWriter CarReturns { get; } = Substitute.For<ICarReturnWriter>();

    public IDispatcher Dispatcher => _scope.ServiceProvider.GetRequiredService<IDispatcher>();

    public void Dispose()
    {
        _scope.Dispose();
        _root.Dispose();
    }
}
