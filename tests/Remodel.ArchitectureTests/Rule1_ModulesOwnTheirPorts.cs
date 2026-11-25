using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace Remodel.ArchitectureTests;

/// <summary>
/// Rule 1: modules own their ports. A module declares the data it needs as interfaces
/// and never references a database library.
/// </summary>
public sealed class Rule1_ModulesOwnTheirPorts
{
    /// <summary>Namespaces of data access libraries. A module's Core may use none of them.</summary>
    private const string DataAccessNamespaces =
        @"^(Microsoft\.EntityFrameworkCore|Dapper|Microsoft\.Data\.SqlClient|System\.Data|Npgsql)(\..+)?$";

    [Theory]
    [MemberData(nameof(Modules.All), MemberType = typeof(Modules))]
    public void Core_does_not_use_data_access_libraries(string module) =>
        Types().That().ResideInAssembly(Modules.Core(module))
            .Should().NotDependOnAnyTypesThat().ResideInNamespaceMatching(DataAccessNamespaces)
            .Check(Modules.Architecture);

    // Proves the rule above can fail: applied to the adapters, which do use data access libraries,
    // it must find violations. Without this, a rule that can't see those dependencies would pass forever.
    [Theory]
    [MemberData(nameof(Modules.All), MemberType = typeof(Modules))]
    public void The_data_access_rule_detects_the_adapters(string module)
    {
        var adapters = Modules.Adapters(module);
        adapters.ShouldNotBeEmpty();

        foreach (var adapter in adapters)
        {
            var rule = Types().That().ResideInAssembly(adapter)
                .Should().NotDependOnAnyTypesThat().ResideInNamespaceMatching(DataAccessNamespaces);

            rule.HasNoViolations(Modules.Architecture).ShouldBeFalse($"{adapter.GetName().Name} should be reported");
        }
    }

    [Theory]
    [MemberData(nameof(Modules.All), MemberType = typeof(Modules))]
    public void Adapters_implement_ports_but_never_declare_them(string module)
    {
        var portsDeclaredInAdapters = Modules.Adapters(module)
            .SelectMany(Conventions.PortsIn)
            .Select(port => port.FullName);

        portsDeclaredInAdapters.ShouldBeEmpty();
    }
}
