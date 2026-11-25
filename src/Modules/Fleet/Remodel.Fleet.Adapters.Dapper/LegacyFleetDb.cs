using Microsoft.Data.SqlClient;

namespace Remodel.Fleet.Adapters.Dapper;

/// <summary>
/// Opens connections to the legacy Fleet database.
/// </summary>
/// <remarks>
/// A typed class instead of a generic "connection factory": when another module uses another database,
/// each adapter still gets the right connection string from DI, with no named registrations.
/// </remarks>
internal sealed class LegacyFleetDb(string connectionString)
{
    public async Task<SqlConnection> OpenAsync(CancellationToken ct)
    {
        var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(ct);
        return connection;
    }
}
