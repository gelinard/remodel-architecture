using System.Data;
using Dapper;
using Remodel.Fleet.Core.Features.GetAvailableCars;

namespace Remodel.Fleet.Adapters.Dapper;

/// <summary>Reads the available cars by calling the existing stored procedure <c>dbo.AvailableCarsGet</c>.</summary>
internal sealed class AvailableCarsReader(LegacyFleetDb db) : IAvailableCarsReader
{
    public async Task<IReadOnlyList<AvailableCar>> Read(CancellationToken ct)
    {
        await using var connection = await db.OpenAsync(ct);

        var rows = await connection.QueryAsync<AvailableCar>(new CommandDefinition(
            "dbo.AvailableCarsGet",
            commandType: CommandType.StoredProcedure,
            cancellationToken: ct));

        return rows.AsList();
    }
}
