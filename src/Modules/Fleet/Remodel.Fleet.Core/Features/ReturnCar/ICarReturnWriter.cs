using Remodel.Dispatching;

namespace Remodel.Fleet.Core.Features.ReturnCar;

/// <summary>Port: saves a car return for <see cref="ReturnCarCommand"/>. Implemented by the Fleet adapters.</summary>
public interface ICarReturnWriter
{
    /// <summary>
    /// Closes the rental and makes the car available again. <paramref name="returnedAt"/> null means "now".
    /// Business problems come back as a failed <see cref="Result"/>, not as exceptions.
    /// </summary>
    Task<Result> Return(int rentalId, int kmIn, DateTime? returnedAt, CancellationToken ct);
}
