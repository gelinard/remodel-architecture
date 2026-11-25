namespace Remodel.Fleet.Core.Features.GetCarHistory;

/// <summary>
/// Port: the data that <see cref="GetCarHistoryQuery"/> needs. Implemented by the Fleet adapters.
/// </summary>
public interface ICarHistoryReader
{
    /// <summary>Rentals of the car, newest pickup first (ties: newest rental first). Deleted rentals are not included.</summary>
    Task<IReadOnlyList<CarHistoryItem>> Read(int carId, CancellationToken ct);
}
