using Microsoft.Extensions.DependencyInjection;
using Remodel.Dispatching;

namespace Remodel.Fleet.Core;

public static class FleetCoreServiceCollectionExtensions
{
    /// <summary>
    /// Registers the Fleet module's handlers and validators. The host must also register exactly one Fleet adapter,
    /// for example <c>AddFleetDapper(...)</c> or <c>AddFleetEfCore(...)</c>, which provides the ports.
    /// </summary>
    public static IServiceCollection AddFleetCore(this IServiceCollection services)
    {
        var assembly = typeof(FleetCoreServiceCollectionExtensions).Assembly;
        return services.AddHandlersFrom(assembly).AddValidatorsFrom(assembly);
    }
}
