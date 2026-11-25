using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Remodel.Bookings.Core.Features.CancelBooking;
using Remodel.Bookings.Core.Features.CreateBooking;
using Remodel.Bookings.Core.Features.GetBooking;

namespace Remodel.Bookings.Adapters.EfCore;

public static class BookingsEfCoreServiceCollectionExtensions
{
    /// <summary>Implements the Bookings module's ports with EF Core on PostgreSQL.</summary>
    public static IServiceCollection AddBookingsEfCore(this IServiceCollection services, string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddDbContextFactory<BookingsDbContext>(options => BookingsDbContext.Configure(options, connectionString));

        services.AddScoped<IBookingWriter, BookingWriter>();
        services.AddScoped<IBookingReader, BookingReader>();
        services.AddScoped<IBookingCancellationWriter, BookingCancellationWriter>();
        return services;
    }
}
