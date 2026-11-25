using Microsoft.EntityFrameworkCore;
using Remodel.Fleet.Core.Features.GetAvailableCars;

namespace Remodel.Fleet.Adapters.EfCore;

/// <summary>
/// Reads the available cars with LINQ over the legacy tables. It must return exactly what
/// <c>dbo.AvailableCarsGet</c> returns; the shared contract tests check that.
/// </summary>
internal sealed class AvailableCarsReader(IDbContextFactory<FleetDbContext> factory) : IAvailableCarsReader
{
    public async Task<IReadOnlyList<AvailableCar>> Read(CancellationToken ct)
    {
        await using var db = await factory.CreateDbContextAsync(ct);

        // Same rules as the stored procedure: marked as available, and no open (not returned, not deleted) rental.
        return await db.Cars
            .Where(car => car.StatusCd == "A")
            .Where(car => !db.Rentals.Any(r => r.CarId == car.Id && r.Returned == null && r.DelFlag == "N"))
            .OrderBy(car => car.RegNo)
            .Select(car => new AvailableCar(car.Id, car.RegNo, car.Model, car.Odometer))
            .ToListAsync(ct);
    }
}
