using Remodel.Dispatching;

namespace Remodel.Fleet.Core.Features.GetCarHistory;

/// <summary>The rental history of one car, newest first. Returns an empty list when the car has no rentals or doesn't exist.</summary>
public sealed record GetCarHistoryQuery(int CarId) : IQuery<IReadOnlyList<CarHistoryItem>>;

/// <summary>One rental in a car's history. <see cref="Returned"/> is null while the car is still out.</summary>
public sealed record CarHistoryItem(int RentalId, string Customer, DateTime PickedUp, DateTime? Returned);
