namespace Remodel.Bookings.Core.Features.CancelBooking;

/// <summary>Port: what <see cref="CancelBookingCommand"/> needs. Implemented by the Bookings adapter.</summary>
public interface IBookingCancellationWriter
{
    /// <summary>The booking's current status, or null when it doesn't exist.</summary>
    Task<BookingStatus?> GetStatus(Guid bookingId, CancellationToken ct);

    Task MarkCancelled(Guid bookingId, CancellationToken ct);
}
