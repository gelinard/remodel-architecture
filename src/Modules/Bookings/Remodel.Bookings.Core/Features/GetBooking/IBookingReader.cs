namespace Remodel.Bookings.Core.Features.GetBooking;

/// <summary>Port: the data that <see cref="GetBookingQuery"/> needs. Implemented by the Bookings adapter.</summary>
public interface IBookingReader
{
    /// <summary>The booking, or null when it doesn't exist.</summary>
    Task<BookingDetails?> Read(Guid bookingId, CancellationToken ct);
}
