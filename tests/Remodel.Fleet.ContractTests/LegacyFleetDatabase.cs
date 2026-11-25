using System.Text.RegularExpressions;
using Dapper;
using Microsoft.Data.SqlClient;
using Remodel.Fleet.ContractTests;
using Testcontainers.MsSql;

[assembly: AssemblyFixture(typeof(LegacyFleetDatabase))]

namespace Remodel.Fleet.ContractTests;

/// <summary>
/// A real SQL Server in Docker with the legacy Fleet schema, procedures and seed data applied.
/// One container is shared by every test in this assembly. Tests stay independent because each one
/// inserts its own cars and customers and only reads those back.
/// </summary>
public sealed partial class LegacyFleetDatabase : IAsyncLifetime
{
    // Same image as docker-compose.yml.
    private const string Image = "mcr.microsoft.com/mssql/server:2022-CU28-ubuntu-22.04";
    private const string DatabaseName = "FleetLegacy";

    private readonly MsSqlContainer _container = new MsSqlBuilder(Image).Build();
    private int _lastCarNumber;

    public string ConnectionString { get; private set; } = "";

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();

        var server = _container.GetConnectionString();
        await using (var master = new SqlConnection(server))
        {
            await master.ExecuteAsync($"CREATE DATABASE [{DatabaseName}]");
        }

        ConnectionString = new SqlConnectionStringBuilder(server) { InitialCatalog = DatabaseName }.ConnectionString;
        await ApplyScripts();
    }

    public ValueTask DisposeAsync() => _container.DisposeAsync();

    public Task<int> AddCustomer(string firstName, string lastName) => InsertAndGetId(
        "INSERT INTO dbo.tbl_Customer (FName, LName) VALUES (@firstName, @lastName)",
        new { firstName, lastName });

    /// <summary>A registration number no other test uses. Tests can add one letter to it (max 10 characters).</summary>
    public string NewRegNo() => $"T-{Interlocked.Increment(ref _lastCarNumber):D5}";

    /// <summary>Adds a car. Status: "A" = available, "R" = rented, "S" = in service.</summary>
    public Task<int> AddCar(string? regNo = null, string status = "A", int odometer = 1000) => InsertAndGetId(
        "INSERT INTO dbo.tbl_Car (CarRegNo, ModelDesc, StatusCd, Odometer) VALUES (@regNo, N'Test car', @status, @odometer)",
        new { regNo = regNo ?? NewRegNo(), status, odometer });

    public Task<int> AddRental(
        int carId, int customerId, DateTime pickedUp, DateTime? returned = null, bool deleted = false, int kmOut = 1000) => InsertAndGetId(
        """
        INSERT INTO dbo.tbl_Rental (CarID, CustID, dtPickup, dtReturn, KmOut, DelFlag)
        VALUES (@carId, @customerId, @pickedUp, @returned, @kmOut, @delFlag)
        """,
        new { carId, customerId, pickedUp, returned, kmOut, delFlag = deleted ? "Y" : "N" });

    public async Task<RentalState> GetRental(int rentalId)
    {
        await using var connection = new SqlConnection(ConnectionString);
        return await connection.QuerySingleAsync<RentalState>(
            "SELECT dtReturn AS Returned, KmIn FROM dbo.tbl_Rental WHERE RentalID = @rentalId", new { rentalId });
    }

    public async Task<CarState> GetCar(int carId)
    {
        await using var connection = new SqlConnection(ConnectionString);
        return await connection.QuerySingleAsync<CarState>(
            "SELECT StatusCd AS Status, Odometer FROM dbo.tbl_Car WHERE CarID = @carId", new { carId });
    }

    private async Task<int> InsertAndGetId(string insertSql, object parameters)
    {
        await using var connection = new SqlConnection(ConnectionString);
        return await connection.ExecuteScalarAsync<int>($"{insertSql}; SELECT CAST(SCOPE_IDENTITY() AS INT);", parameters);
    }

    /// <summary>Runs db/fleet-legacy/*.sql in file-name order, split into batches on "GO" lines like sqlcmd does.</summary>
    private async Task ApplyScripts()
    {
        var scripts = Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "fleet-legacy"), "*.sql").Order();

        await using var connection = new SqlConnection(ConnectionString);
        foreach (var script in scripts)
        {
            var batches = GoLine().Split(await File.ReadAllTextAsync(script));
            foreach (var batch in batches.Where(b => !string.IsNullOrWhiteSpace(b)))
            {
                await connection.ExecuteAsync(batch);
            }
        }
    }

    [GeneratedRegex(@"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase)]
    private static partial Regex GoLine();
}

/// <summary>The columns of a <c>tbl_Rental</c> row that a car return changes.</summary>
public sealed record RentalState(DateTime? Returned, int? KmIn);

/// <summary>The columns of a <c>tbl_Car</c> row that a car return changes.</summary>
public sealed record CarState(string Status, int Odometer);
