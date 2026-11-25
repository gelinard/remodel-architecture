using System.Data;
using Dapper;
using Remodel.Fleet.Core.Features.GetCarHistory;

namespace Remodel.Fleet.Adapters.Dapper;

/// <summary>Reads a car's history by calling the existing stored procedure <c>dbo.CarHistoryGet</c>.</summary>
internal sealed class CarHistoryReader(LegacyFleetDb db) : ICarHistoryReader
{
    public async Task<IReadOnlyList<CarHistoryItem>> Read(int carId, CancellationToken ct)
    {
        await using var connection = await db.OpenAsync(ct);

        // The procedure's column aliases match the record's constructor, so Dapper maps rows straight into it.
        var rows = await connection.QueryAsync<CarHistoryItem>(new CommandDefinition(
            "dbo.CarHistoryGet",
            new { CarId = carId },
            commandType: CommandType.StoredProcedure,
            cancellationToken: ct));

        return rows.AsList();
    }
}
