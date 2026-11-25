using Remodel.Dispatching;

namespace Remodel.Bookings.Core.Features.CreateBooking;

/// <summary>Books a car for a period of days. Returns the new booking's id.</summary>
/// <remarks>
/// Checking that the car exists and is free needs the Fleet module. That is not part of v1 (see ADR 14).
/// </remarks>
public sealed record CreateBookingCommand(int CarId, string CustomerName, DateOnly PickupDate, DateOnly ReturnDate)
    : ICommand<Guid>;

/// <summary>A booking to store, with the id chosen by the handler.</summary>
public sealed record NewBooking(Guid Id, int CarId, string CustomerName, DateOnly PickupDate, DateOnly ReturnDate);
