using Remodel.Dispatching;

namespace Remodel.Fleet.Core.Features.GetAvailableCars;

/// <summary>Cars that can be rented right now, sorted by registration number.</summary>
public sealed record GetAvailableCarsQuery : IQuery<IReadOnlyList<AvailableCar>>;

/// <summary>A car that can be rented now. <see cref="Odometer"/> is in km.</summary>
public sealed record AvailableCar(int CarId, string RegNo, string Model, int Odometer);
