using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Remodel.Bookings.Adapters.EfCore;
using Remodel.Bookings.Core;

namespace Remodel.Host.Functions;

internal static class BookingsModule
{
    /// <summary>Registers the Bookings module with its EF Core adapter on PostgreSQL (connection string <c>Bookings</c>).</summary>
    public static IServiceCollection AddBookings(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Bookings")
            ?? throw new InvalidOperationException("The connection string 'Bookings' is missing.");

        return services.AddBookingsCore().AddBookingsEfCore(connectionString);
    }
}
