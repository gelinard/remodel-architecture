using System.Data;
using Remodel.Dispatching;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace Remodel.ArchitectureTests;

/// <summary>
/// Rule 3: one port per use case. <c>...Reader</c> ports serve queries, <c>...Writer</c> ports serve commands.
/// No generic repositories.
/// </summary>
public sealed class Rule3_OnePortPerUseCase
{
    [Theory]
    [MemberData(nameof(Modules.All), MemberType = typeof(Modules))]
    public void Query_handlers_never_use_writers(string module) =>
        Classes().That().ResideInAssembly(Modules.Core(module)).And().HaveNameEndingWith("QueryHandler")
            .Should().NotDependOnAnyTypesThat().HaveNameEndingWith("Writer")
            .WithoutRequiringPositiveResults()
            .Check(Modules.Architecture);

    [Theory]
    [MemberData(nameof(Modules.All), MemberType = typeof(Modules))]
    public void Interfaces_in_feature_folders_are_ports_named_reader_or_writer(string module)
    {
        var badlyNamed = Modules.Core(module).GetTypes()
            .Where(type => type.IsInterface && type.Namespace?.Contains(".Features.", StringComparison.Ordinal) == true)
            .Where(type => !Conventions.IsPort(type))
            .Select(type => type.FullName);

        badlyNamed.ShouldBeEmpty();
    }

    [Theory]
    [MemberData(nameof(Modules.All), MemberType = typeof(Modules))]
    public void Ports_are_specific_not_generic(string module)
    {
        var genericPorts = Conventions.PortsIn(Modules.Core(module))
            .Where(port => port.IsGenericTypeDefinition)
            .Select(port => port.FullName);

        genericPorts.ShouldBeEmpty();
    }

    [Theory]
    [MemberData(nameof(Modules.All), MemberType = typeof(Modules))]
    public void There_are_no_repositories(string module)
    {
        var repositories = Modules.AllAssemblies(module)
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => Conventions.NameOf(type).EndsWith("Repository", StringComparison.Ordinal))
            .Select(type => type.FullName);

        repositories.ShouldBeEmpty();
    }

    [Theory]
    [MemberData(nameof(Modules.All), MemberType = typeof(Modules))]
    public void Every_port_method_takes_a_cancellation_token(string module)
    {
        var methodsWithoutToken = Conventions.PortsIn(Modules.Core(module))
            .SelectMany(port => port.GetMethods())
            .Where(method => !method.GetParameters().Any(p => p.ParameterType == typeof(CancellationToken)))
            .Select(method => $"{method.DeclaringType!.Name}.{method.Name}");

        methodsWithoutToken.ShouldBeEmpty();
    }

    [Theory]
    [MemberData(nameof(Modules.All), MemberType = typeof(Modules))]
    public void Ports_return_module_records_not_queryables_dictionaries_or_data_tables(string module)
    {
        Type[] forbidden =
        [
            typeof(IQueryable), typeof(IQueryable<>),
            typeof(Dictionary<,>), typeof(IDictionary<,>), typeof(IReadOnlyDictionary<,>),
            typeof(DataTable), typeof(DataSet),
        ];

        var leaks = Conventions.PortsIn(Modules.Core(module))
            .SelectMany(port => port.GetMethods())
            .Where(method => TypeAndArguments(method.ReturnType).Any(type => forbidden.Contains(Definition(type))))
            .Select(method => $"{method.DeclaringType!.Name}.{method.Name} returns {method.ReturnType}");

        leaks.ShouldBeEmpty();

        static IEnumerable<Type> TypeAndArguments(Type type) =>
            type.IsGenericType ? type.GetGenericArguments().SelectMany(TypeAndArguments).Prepend(type) : [type];

        static Type Definition(Type type) => type.IsGenericType ? type.GetGenericTypeDefinition() : type;
    }

    // The rules above find handlers and ports by name, so the names must be trustworthy:
    // anything that implements a query, command, handler or validator interface must carry the matching suffix.
    [Theory]
    [MemberData(nameof(Modules.All), MemberType = typeof(Modules))]
    public void Messages_and_handlers_are_named_after_their_role(string module)
    {
        var suffixes = new Dictionary<Type, string>
        {
            [typeof(IQuery<>)] = "Query",
            [typeof(ICommand<>)] = "Command",
            [typeof(IQueryHandler<,>)] = "QueryHandler",
            [typeof(ICommandHandler<,>)] = "CommandHandler",
            [typeof(IValidator<>)] = "Validator",
        };

        var misnamed =
            from type in Modules.Core(module).GetTypes()
            from contract in type.GetInterfaces()
            where contract.IsGenericType && suffixes.ContainsKey(contract.GetGenericTypeDefinition())
            let suffix = suffixes[contract.GetGenericTypeDefinition()]
            where !type.Name.EndsWith(suffix, StringComparison.Ordinal)
            select $"{type.Name} should end with \"{suffix}\"";

        misnamed.ShouldBeEmpty();
    }
}
