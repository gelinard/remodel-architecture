using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Remodel.Bookings.Adapters.EfCore;

/// <summary>
/// Lets <c>dotnet ef migrations add</c> create the DbContext without starting a host.
/// Adding a migration never connects to the database, so the connection string is only a placeholder.
/// </summary>
internal sealed class BookingsDesignTimeFactory : IDesignTimeDbContextFactory<BookingsDbContext>
{
    public BookingsDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<BookingsDbContext>();
        BookingsDbContext.Configure(options, "Host=localhost;Database=bookings");
        return new BookingsDbContext(options.Options);
    }
}
