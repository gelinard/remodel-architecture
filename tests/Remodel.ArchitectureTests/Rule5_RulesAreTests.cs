using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace Remodel.ArchitectureTests;

/// <summary>
/// Rule 5: rules are tests. The project references already stop most wrong dependencies at compile time.
/// These tests keep it that way if someone adds a reference, and check the rules the compiler can't.
/// </summary>
public sealed class Rule5_RulesAreTests
{
    [Fact]
    public void Modules_are_found_by_convention() =>
        Modules.Names.ShouldContain("Fleet");

    // Core must work the same behind any host, so it can't know about adapters, hosts or web frameworks.
    [Theory]
    [MemberData(nameof(Modules.All), MemberType = typeof(Modules))]
    public void Core_does_not_depend_on_adapters_or_hosts(string module) =>
        Types().That().ResideInAssembly(Modules.Core(module))
            .Should().NotDependOnAnyTypesThat().ResideInNamespaceMatching(
                @"^(Remodel\.(\w+\.Adapters|Host)|Microsoft\.AspNetCore|Microsoft\.Azure\.Functions)(\..+)?$")
            .Check(Modules.Architecture);

    [Theory]
    [MemberData(nameof(Modules.All), MemberType = typeof(Modules))]
    public void Modules_do_not_use_each_other(string module)
    {
        var others = Modules.Names.Where(name => name != module).ToList();
        Assert.SkipWhen(others.Count == 0, "Only one module so far. This rule starts checking when a second module is added.");

        foreach (var assembly in Modules.AllAssemblies(module))
        {
            Types().That().ResideInAssembly(assembly)
                .Should().NotDependOnAnyTypesThat().ResideInNamespaceMatching($@"^Remodel\.({string.Join('|', others)})(\..+)?$")
                .Check(Modules.Architecture);
        }
    }

    [Theory]
    [MemberData(nameof(Modules.All), MemberType = typeof(Modules))]
    public void Handlers_and_validators_are_internal_so_only_the_dispatcher_uses_them(string module) =>
        Classes().That().ResideInAssembly(Modules.Core(module)).And().HaveNameMatching("(Handler|Validator)$")
            .Should().NotBePublic()
            .Check(Modules.Architecture);

    // A host uses an adapter only through static entry points, such as AddFleetDapper(...) or BookingsMigrations.ApplyAsync(...).
    // The classes that implement the ports stay internal. EF Core migrations are allowed: "dotnet ef" generates them as public.
    [Theory]
    [MemberData(nameof(Modules.All), MemberType = typeof(Modules))]
    public void Adapters_expose_only_static_entry_points(string module)
    {
        var publicTypes = Modules.Adapters(module)
            .SelectMany(adapter => adapter.GetExportedTypes())
            .Where(type => !IsStaticClass(type) && !IsEfCoreMigration(type))
            .Select(type => type.FullName);

        publicTypes.ShouldBeEmpty();

        static bool IsStaticClass(Type type) => type is { IsAbstract: true, IsSealed: true };

        static bool IsEfCoreMigration(Type type) => type.BaseType?.FullName == "Microsoft.EntityFrameworkCore.Migrations.Migration";
    }
}
