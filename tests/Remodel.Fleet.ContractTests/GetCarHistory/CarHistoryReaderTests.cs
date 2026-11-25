using Remodel.Fleet.Adapters.Dapper;
using Remodel.Fleet.Adapters.EfCore;

namespace Remodel.Fleet.ContractTests.GetCarHistory;

// The same contract, run once per adapter. Adding an adapter means adding one line here.

public sealed class DapperCarHistoryReaderTests(LegacyFleetDatabase database)
    : CarHistoryReaderContract(database, (services, connectionString) => services.AddFleetDapper(connectionString));

public sealed class EfCoreCarHistoryReaderTests(LegacyFleetDatabase database)
    : CarHistoryReaderContract(database, (services, connectionString) => services.AddFleetEfCore(connectionString));
