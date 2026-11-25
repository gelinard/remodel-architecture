using Microsoft.Extensions.DependencyInjection;
using Remodel.Bookings.Adapters.EfCore;
using Remodel.Bookings.ContractTests;
using Testcontainers.PostgreSql;

[assembly: AssemblyFixture(typeof(BookingsDatabase))]

namespace Remodel.Bookings.ContractTests;

/// <summary>
/// A real PostgreSQL in Docker, with the schema created by the adapter's own EF Core migrations,
/// so every test run also tests the migrations. One container is shared by every test in this assembly;
/// each test creates its own bookings, so tests don't depend on each other.
/// </summary>
public sealed class BookingsDatabase : IAsyncLifetime
{
    // Same image as docker-compose.yml.
    private const string Image = "postgres:18.7";

    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder(Image).Build();

    public string ConnectionString { get; private set; } = "";

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();
        ConnectionString = _container.GetConnectionString();

        await using var services = new ServiceCollection().AddBookingsEfCore(ConnectionString).BuildServiceProvider();
        await BookingsMigrations.ApplyAsync(services, TestContext.Current.CancellationToken);
    }

    public ValueTask DisposeAsync() => _container.DisposeAsync();
}
