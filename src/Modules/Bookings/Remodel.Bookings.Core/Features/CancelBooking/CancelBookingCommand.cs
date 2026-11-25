using Remodel.Dispatching;

namespace Remodel.Bookings.Core.Features.CancelBooking;

/// <summary>
/// Cancels a booking. Fails with <see cref="ErrorKind.NotFound"/> when it doesn't exist
/// and with <see cref="ErrorKind.Conflict"/> when it is already cancelled.
/// </summary>
public sealed record CancelBookingCommand(Guid BookingId) : ICommand<Result>;
