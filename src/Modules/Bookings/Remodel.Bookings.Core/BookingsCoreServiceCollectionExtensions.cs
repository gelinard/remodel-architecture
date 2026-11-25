using Microsoft.Extensions.DependencyInjection;
using Remodel.Dispatching;

namespace Remodel.Bookings.Core;

public static class BookingsCoreServiceCollectionExtensions
{
    /// <summary>
    /// Registers the Bookings module's handlers and validators. The host must also register a Bookings adapter,
    /// for example <c>AddBookingsEfCore(...)</c>, which provides the ports.
    /// </summary>
    public static IServiceCollection AddBookingsCore(this IServiceCollection services)
    {
        var assembly = typeof(BookingsCoreServiceCollectionExtensions).Assembly;
        return services.AddHandlersFrom(assembly).AddValidatorsFrom(assembly);
    }
}
