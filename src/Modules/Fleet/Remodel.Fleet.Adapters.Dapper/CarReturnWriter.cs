using System.Data;
using Dapper;
using Remodel.Dispatching;
using Remodel.Fleet.Core.Features.ReturnCar;

namespace Remodel.Fleet.Adapters.Dapper;

/// <summary>Saves a car return with the existing stored procedure <c>dbo.CarReturn</c>.</summary>
internal sealed class CarReturnWriter(LegacyFleetDb db) : ICarReturnWriter
{
    public async Task<Result> Return(int rentalId, int kmIn, DateTime? returnedAt, CancellationToken ct)
    {
        var parameters = new DynamicParameters();
        parameters.Add("RentalId", rentalId);
        parameters.Add("KmIn", kmIn);
        parameters.Add("dtReturn", returnedAt, DbType.DateTime);
        parameters.Add("ErrMsg", dbType: DbType.String, size: 400, direction: ParameterDirection.Output);
        parameters.Add("ReturnCode", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);

        await using var connection = await db.OpenAsync(ct);
        await connection.ExecuteAsync(new CommandDefinition(
            "dbo.CarReturn",
            parameters,
            commandType: CommandType.StoredProcedure,
            cancellationToken: ct));

        return ToResult(parameters.Get<int>("ReturnCode"), parameters.Get<string?>("ErrMsg"));
    }

    // Legacy convention: the procedure returns 0 when it worked. Otherwise it returns an error code and puts
    // a message in @ErrMsg: 1 = rental not found, 2 = already returned, 3 and 4 = km or time is wrong.
    // The EF Core adapter has the same mapping; the contract tests keep the two in line.
    private static Result ToResult(int returnCode, string? errMsg) => returnCode switch
    {
        0 => Result.Success,
        1 => Result.Failure(ErrorKind.NotFound, errMsg ?? "Rental not found."),
        2 => Result.Failure(ErrorKind.Conflict, errMsg ?? "The car has already been returned."),
        _ => Result.Failure(ErrorKind.Rejected, errMsg ?? $"dbo.CarReturn failed with code {returnCode}."),
    };
}
