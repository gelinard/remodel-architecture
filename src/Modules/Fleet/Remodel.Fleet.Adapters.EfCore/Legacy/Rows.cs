namespace Remodel.Fleet.Adapters.EfCore.Legacy;

// Rows of the legacy tables, with clean names. FleetDbContext maps them to the real table and column names.
// They are not domain objects and never leave this project: readers project them into the module's records.
// Only the columns that the adapter uses are mapped.

/// <summary>A row in <c>dbo.tbl_Rental</c>.</summary>
internal sealed class RentalRow
{
    public int Id { get; set; }

    public int CarId { get; set; }

    public int CustomerId { get; set; }

    public CustomerRow Customer { get; set; } = null!;

    public DateTime PickedUp { get; set; }

    public DateTime? Returned { get; set; }

    /// <summary>"Y" = deleted, "N" = active. The legacy system never deletes rows.</summary>
    public string DelFlag { get; set; } = "N";
}

/// <summary>A row in <c>dbo.tbl_Car</c>.</summary>
internal sealed class CarRow
{
    public int Id { get; set; }

    public string RegNo { get; set; } = "";

    public string Model { get; set; } = "";

    /// <summary>"A" = available, "R" = rented, "S" = in service.</summary>
    public string StatusCd { get; set; } = "A";

    public int Odometer { get; set; }
}

/// <summary>A row in <c>dbo.tbl_Customer</c>.</summary>
internal sealed class CustomerRow
{
    public int Id { get; set; }

    public string FirstName { get; set; } = "";

    public string LastName { get; set; } = "";
}
