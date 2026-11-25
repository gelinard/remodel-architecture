using Microsoft.EntityFrameworkCore;
using Remodel.Fleet.Core.Features.GetCarHistory;

namespace Remodel.Fleet.Adapters.EfCore;

/// <summary>
/// Reads a car's history with LINQ over the legacy tables. It must return exactly what
/// <c>dbo.CarHistoryGet</c> returns; the shared contract tests check that.
/// </summary>
internal sealed class CarHistoryReader(IDbContextFactory<FleetDbContext> factory) : ICarHistoryReader
{
    public async Task<IReadOnlyList<CarHistoryItem>> Read(int carId, CancellationToken ct)
    {
        await using var db = await factory.CreateDbContextAsync(ct);

        return await db.Rentals
            .Where(r => r.CarId == carId && r.DelFlag == "N") // same rule as the stored procedure: hide deleted rentals
            .OrderByDescending(r => r.PickedUp)
            .ThenByDescending(r => r.Id)
            .Select(r => new CarHistoryItem(
                r.Id,
                r.Customer.FirstName + " " + r.Customer.LastName,
                r.PickedUp,
                r.Returned))
            .ToListAsync(ct);
    }
}
