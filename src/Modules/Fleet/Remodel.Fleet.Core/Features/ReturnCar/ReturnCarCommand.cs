using Remodel.Dispatching;

namespace Remodel.Fleet.Core.Features.ReturnCar;

/// <summary>
/// Registers that a rented car came back. <see cref="ReturnedAt"/> is optional: without it, the current time is used.
/// </summary>
/// <remarks>
/// Expected failures come back as a <see cref="Result"/>: the rental doesn't exist (<see cref="ErrorKind.NotFound"/>),
/// the car is already returned (<see cref="ErrorKind.Conflict"/>), or the km or time is wrong (<see cref="ErrorKind.Rejected"/>).
/// </remarks>
public sealed record ReturnCarCommand(int RentalId, int KmIn, DateTime? ReturnedAt = null) : ICommand<Result>;
