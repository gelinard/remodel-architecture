using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Remodel.Bookings.Adapters.EfCore;

/// <summary>Creates or updates the Bookings schema with the EF Core migrations in this project.</summary>
public static class BookingsMigrations
{
    /// <summary>
    /// Applies every migration that hasn't run yet. Needs <c>AddBookingsEfCore(...)</c> first.
    /// EF Core refuses to migrate when the model has changes that no migration covers,
    /// so a forgotten "dotnet ef migrations add" shows up as an error, not as a wrong schema.
    /// </summary>
    public static async Task ApplyAsync(IServiceProvider services, CancellationToken ct)
    {
        var factory = services.GetRequiredService<IDbContextFactory<BookingsDbContext>>();
        await using var db = await factory.CreateDbContextAsync(ct);
        await db.Database.MigrateAsync(ct);
    }
}
