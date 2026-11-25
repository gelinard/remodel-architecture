using Remodel.Dispatching;

namespace Remodel.Fleet.Core.Features.GetAvailableCars;

internal sealed class GetAvailableCarsQueryHandler(IAvailableCarsReader reader)
    : IQueryHandler<GetAvailableCarsQuery, IReadOnlyList<AvailableCar>>
{
    public Task<IReadOnlyList<AvailableCar>> Handle(GetAvailableCarsQuery query, CancellationToken ct)
        => reader.Read(ct);
}
