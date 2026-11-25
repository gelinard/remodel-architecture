using Remodel.Dispatching;

namespace Remodel.Fleet.Core.Features.GetCarHistory;

internal sealed class GetCarHistoryQueryHandler(ICarHistoryReader reader)
    : IQueryHandler<GetCarHistoryQuery, IReadOnlyList<CarHistoryItem>>
{
    public Task<IReadOnlyList<CarHistoryItem>> Handle(GetCarHistoryQuery query, CancellationToken ct)
        => reader.Read(query.CarId, ct);
}
