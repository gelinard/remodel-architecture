namespace Remodel.Fleet.Core.Features.GetAvailableCars;

/// <summary>Port: the data that <see cref="GetAvailableCarsQuery"/> needs. Implemented by the Fleet adapters.</summary>
public interface IAvailableCarsReader
{
    /// <summary>Cars marked as available that have no open rental, sorted by registration number.</summary>
    Task<IReadOnlyList<AvailableCar>> Read(CancellationToken ct);
}
