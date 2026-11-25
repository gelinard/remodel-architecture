using Remodel.Dispatching;

namespace Remodel.Bookings.Core.Features.GetBooking;

/// <summary>One booking, or null when it doesn't exist.</summary>
public sealed record GetBookingQuery(Guid BookingId) : IQuery<BookingDetails?>;

public sealed record BookingDetails(
    Guid Id, int CarId, string CustomerName, DateOnly PickupDate, DateOnly ReturnDate, BookingStatus Status);
