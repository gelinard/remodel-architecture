using Remodel.Fleet.Adapters.Dapper;
using Remodel.Fleet.Adapters.EfCore;

namespace Remodel.Fleet.ContractTests.GetAvailableCars;

public sealed class DapperAvailableCarsReaderTests(LegacyFleetDatabase database)
    : AvailableCarsReaderContract(database, (services, connectionString) => services.AddFleetDapper(connectionString));

public sealed class EfCoreAvailableCarsReaderTests(LegacyFleetDatabase database)
    : AvailableCarsReaderContract(database, (services, connectionString) => services.AddFleetEfCore(connectionString));
