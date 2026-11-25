using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Remodel.Fleet.Adapters.Dapper;
using Remodel.Fleet.Adapters.EfCore;
using Remodel.Fleet.Core;

namespace Remodel.Host.Functions;

internal static class FleetModule
{
    /// <summary>
    /// Registers the Fleet module with the adapter named in configuration:
    /// <c>Fleet:Adapter</c> = <c>Dapper</c> or <c>EfCore</c>. Switching is a configuration change, not a code change.
    /// </summary>
    public static IServiceCollection AddFleet(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("FleetLegacy")
            ?? throw new InvalidOperationException("The connection string 'FleetLegacy' is missing.");

        services.AddFleetCore();

        return configuration["Fleet:Adapter"] switch
        {
            "Dapper" => services.AddFleetDapper(connectionString),
            "EfCore" => services.AddFleetEfCore(connectionString),
            var other => throw new InvalidOperationException($"Fleet:Adapter must be \"Dapper\" or \"EfCore\", not \"{other}\"."),
        };
    }
}
