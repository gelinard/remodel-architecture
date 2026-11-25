using System.Reflection;
using ArchUnitNET.Fluent;
using Architecture = ArchUnitNET.Domain.Architecture;

namespace Remodel.ArchitectureTests;

/// <summary>Small helpers shared by the rule classes.</summary>
internal static class Conventions
{
    /// <summary>A port is an interface named <c>I...Reader</c> (serves queries) or <c>I...Writer</c> (serves commands).</summary>
    public static bool IsPort(Type type) =>
        type.IsInterface && (NameOf(type).EndsWith("Reader", StringComparison.Ordinal) || NameOf(type).EndsWith("Writer", StringComparison.Ordinal));

    public static IEnumerable<Type> PortsIn(Assembly assembly) => assembly.GetTypes().Where(IsPort);

    /// <summary>The type name without the generic arity suffix: <c>IReader`1</c> becomes <c>IReader</c>.</summary>
    public static string NameOf(Type type) => type.Name.Split('`')[0];

    /// <summary>
    /// Fails the test and lists every violation. The ArchUnitNET xUnit package does the same,
    /// but it is built against an old xUnit assertion package, so this small helper avoids that dependency.
    /// </summary>
    public static void Check(this IArchRule rule, Architecture architecture)
    {
        var violations = rule.Evaluate(architecture)
            .Where(result => !result.Passed)
            .Select(result => result.Description)
            .ToList();

        violations.ShouldBeEmpty($"Rule broken: {rule.Description}");
    }
}
