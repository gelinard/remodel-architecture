using Remodel.Fleet.Adapters.Dapper;
using Remodel.Fleet.Adapters.EfCore;

namespace Remodel.Fleet.ContractTests.ReturnCar;

public sealed class DapperCarReturnWriterTests(LegacyFleetDatabase database)
    : CarReturnWriterContract(database, (services, connectionString) => services.AddFleetDapper(connectionString));

public sealed class EfCoreCarReturnWriterTests(LegacyFleetDatabase database)
    : CarReturnWriterContract(database, (services, connectionString) => services.AddFleetEfCore(connectionString));
