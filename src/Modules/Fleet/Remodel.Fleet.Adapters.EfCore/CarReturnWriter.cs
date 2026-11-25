using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Remodel.Dispatching;
using Remodel.Fleet.Core.Features.ReturnCar;

namespace Remodel.Fleet.Adapters.EfCore;

/// <summary>
/// Saves a car return by calling the legacy stored procedure <c>dbo.CarReturn</c> through EF Core.
/// The business rules for a return live in that procedure, so this adapter doesn't copy them into LINQ.
/// </summary>
internal sealed class CarReturnWriter(IDbContextFactory<FleetDbContext> factory) : ICarReturnWriter
{
    public async Task<Result> Return(int rentalId, int kmIn, DateTime? returnedAt, CancellationToken ct)
    {
        var errMsg = new SqlParameter("@ErrMsg", SqlDbType.NVarChar, 400) { Direction = ParameterDirection.Output };
        var returnCode = new SqlParameter("@ReturnCode", SqlDbType.Int) { Direction = ParameterDirection.Output };
        SqlParameter[] parameters =
        [
            new("@RentalId", rentalId),
            new("@KmIn", kmIn),
            new("@dtReturn", SqlDbType.DateTime) { Value = (object?)returnedAt ?? DBNull.Value },
            errMsg,
            returnCode,
        ];

        await using var db = await factory.CreateDbContextAsync(ct);
        await db.Database.ExecuteSqlRawAsync(
            "EXEC @ReturnCode = dbo.CarReturn @RentalId = @RentalId, @KmIn = @KmIn, @dtReturn = @dtReturn, @ErrMsg = @ErrMsg OUTPUT",
            parameters,
            ct);

        return ToResult((int)returnCode.Value, errMsg.Value as string);
    }

    // Legacy convention: the procedure returns 0 when it worked. Otherwise it returns an error code and puts
    // a message in @ErrMsg: 1 = rental not found, 2 = already returned, 3 and 4 = km or time is wrong.
    // The Dapper adapter has the same mapping; the contract tests keep the two in line.
    private static Result ToResult(int returnCode, string? errMsg) => returnCode switch
    {
        0 => Result.Success,
        1 => Result.Failure(ErrorKind.NotFound, errMsg ?? "Rental not found."),
        2 => Result.Failure(ErrorKind.Conflict, errMsg ?? "The car has already been returned."),
        _ => Result.Failure(ErrorKind.Rejected, errMsg ?? $"dbo.CarReturn failed with code {returnCode}."),
    };
}
