using Microsoft.EntityFrameworkCore;
using Remodel.Fleet.Adapters.EfCore.Legacy;

namespace Remodel.Fleet.Adapters.EfCore;

/// <summary>
/// Database-first mapping of the legacy Fleet tables. The schema is owned by the SQL scripts in db/fleet-legacy,
/// so there are no EF Core migrations here.
/// </summary>
internal sealed class FleetDbContext(DbContextOptions<FleetDbContext> options) : DbContext(options)
{
    public DbSet<RentalRow> Rentals => Set<RentalRow>();

    public DbSet<CarRow> Cars => Set<CarRow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CarRow>(car =>
        {
            car.ToTable("tbl_Car", "dbo");
            car.HasKey(c => c.Id);
            car.Property(c => c.Id).HasColumnName("CarID");
            car.Property(c => c.RegNo).HasColumnName("CarRegNo").HasColumnType("varchar(10)");
            car.Property(c => c.Model).HasColumnName("ModelDesc").HasMaxLength(100);
            car.Property(c => c.StatusCd).HasColumnName("StatusCd").HasColumnType("char(1)");
            car.Property(c => c.Odometer).HasColumnName("Odometer");
        });

        modelBuilder.Entity<RentalRow>(rental =>
        {
            rental.ToTable("tbl_Rental", "dbo");
            rental.HasKey(r => r.Id);
            rental.Property(r => r.Id).HasColumnName("RentalID");
            rental.Property(r => r.CarId).HasColumnName("CarID");
            rental.Property(r => r.CustomerId).HasColumnName("CustID");
            rental.Property(r => r.PickedUp).HasColumnName("dtPickup").HasColumnType("datetime");
            rental.Property(r => r.Returned).HasColumnName("dtReturn").HasColumnType("datetime");
            rental.Property(r => r.DelFlag).HasColumnName("DelFlag").HasColumnType("char(1)");
            rental.HasOne(r => r.Customer).WithMany().HasForeignKey(r => r.CustomerId);
        });

        modelBuilder.Entity<CustomerRow>(customer =>
        {
            customer.ToTable("tbl_Customer", "dbo");
            customer.HasKey(c => c.Id);
            customer.Property(c => c.Id).HasColumnName("CustID");
            customer.Property(c => c.FirstName).HasColumnName("FName").HasMaxLength(50);
            customer.Property(c => c.LastName).HasColumnName("LName").HasMaxLength(50);
        });
    }
}
