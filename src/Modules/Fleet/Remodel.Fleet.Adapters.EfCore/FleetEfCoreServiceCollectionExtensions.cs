using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Remodel.Fleet.Core.Features.GetAvailableCars;
using Remodel.Fleet.Core.Features.GetCarHistory;
using Remodel.Fleet.Core.Features.ReturnCar;

namespace Remodel.Fleet.Adapters.EfCore;

public static class FleetEfCoreServiceCollectionExtensions
{
    /// <summary>Implements the Fleet module's ports with EF Core, mapped onto the legacy tables.</summary>
    public static IServiceCollection AddFleetEfCore(this IServiceCollection services, string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        // A factory gives each port call its own short-lived DbContext, which works the same
        // in HTTP requests, queue triggers and singletons, and never shares a context between threads.
        services.AddDbContextFactory<FleetDbContext>(options => options
            .UseSqlServer(connectionString)
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

        services.AddScoped<ICarHistoryReader, CarHistoryReader>();
        services.AddScoped<IAvailableCarsReader, AvailableCarsReader>();
        services.AddScoped<ICarReturnWriter, CarReturnWriter>();
        return services;
    }
}
