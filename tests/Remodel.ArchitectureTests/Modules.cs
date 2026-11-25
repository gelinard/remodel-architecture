using System.Reflection;
using ArchUnitNET.Loader;
using Architecture = ArchUnitNET.Domain.Architecture;

namespace Remodel.ArchitectureTests;

/// <summary>
/// The modules under test, found by convention: every <c>Remodel.{Module}.Core.dll</c> next to the tests is a module,
/// and every <c>Remodel.{Module}.Adapters.*.dll</c> is one of its adapters.
/// </summary>
internal static class Modules
{
    public static IReadOnlyList<string> Names { get; } =
        FindAssemblies("Remodel.*.Core.dll")
            .Select(name => name["Remodel.".Length..^".Core".Length])
            .Order()
            .ToList();

    /// <summary>Test data for <c>[MemberData]</c>: one test case per module.</summary>
    public static TheoryData<string> All => new(Names);

    /// <summary>All module assemblies, loaded once for the whole run because loading is the slow part.</summary>
    public static Architecture Architecture { get; } = new ArchLoader()
        .LoadAssemblies(Names.SelectMany(module => Adapters(module).Prepend(Core(module))).ToArray())
        .Build();

    public static Assembly Core(string module) => Assembly.Load($"Remodel.{module}.Core");

    public static IReadOnlyList<Assembly> Adapters(string module) =>
        FindAssemblies($"Remodel.{module}.Adapters.*.dll").Select(Assembly.Load).ToList();

    public static IEnumerable<Assembly> AllAssemblies(string module) => Adapters(module).Prepend(Core(module));

    private static IEnumerable<string> FindAssemblies(string fileNamePattern) =>
        Directory.GetFiles(AppContext.BaseDirectory, fileNamePattern).Select(file => Path.GetFileNameWithoutExtension(file));
}
