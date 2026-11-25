using Microsoft.Extensions.DependencyInjection;
using Remodel.Fleet.Core.Features.GetAvailableCars;
using Remodel.Fleet.Core.Features.GetCarHistory;
using Remodel.Fleet.Core.Features.ReturnCar;

namespace Remodel.Fleet.Adapters.Dapper;

public static class FleetDapperServiceCollectionExtensions
{
    /// <summary>Implements the Fleet module's ports with Dapper and the legacy stored procedures.</summary>
    public static IServiceCollection AddFleetDapper(this IServiceCollection services, string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddSingleton(new LegacyFleetDb(connectionString));
        services.AddScoped<ICarHistoryReader, CarHistoryReader>();
        services.AddScoped<IAvailableCarsReader, AvailableCarsReader>();
        services.AddScoped<ICarReturnWriter, CarReturnWriter>();
        return services;
    }
}
