namespace Remodel.Bookings.Core.Features.CreateBooking;

/// <summary>Port: stores new bookings for <see cref="CreateBookingCommand"/>. Implemented by the Bookings adapter.</summary>
public interface IBookingWriter
{
    /// <summary>Stores the booking with status <see cref="BookingStatus.Active"/>.</summary>
    Task Add(NewBooking booking, CancellationToken ct);
}
