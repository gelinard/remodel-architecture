using Microsoft.EntityFrameworkCore;

namespace Remodel.Bookings.Adapters.EfCore;

/// <summary>
/// Code-first: this module owns its database, so the schema comes from this model and the migrations in /Migrations.
/// Everything lives in the "bookings" schema with snake_case names, the usual PostgreSQL style.
/// </summary>
internal sealed class BookingsDbContext(DbContextOptions<BookingsDbContext> options) : DbContext(options)
{
    public const string Schema = "bookings";

    public DbSet<BookingRow> Bookings => Set<BookingRow>();

    /// <summary>The same options at runtime and at design time (dotnet ef), so both use the same migrations history table.</summary>
    public static void Configure(DbContextOptionsBuilder options, string connectionString) =>
        options.UseNpgsql(connectionString, npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", Schema));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);

        modelBuilder.Entity<BookingRow>(booking =>
        {
            booking.ToTable("bookings");
            booking.HasKey(b => b.Id);
            booking.Property(b => b.Id).HasColumnName("id").ValueGeneratedNever(); // the handler chooses the id
            booking.Property(b => b.CarId).HasColumnName("car_id");
            booking.Property(b => b.CustomerName).HasColumnName("customer_name").HasMaxLength(100);
            booking.Property(b => b.PickupDate).HasColumnName("pickup_date");
            booking.Property(b => b.ReturnDate).HasColumnName("return_date");
            booking.Property(b => b.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20);
        });
    }
}
