using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Remodel.Dispatching;

public static class DispatchingServiceCollectionExtensions
{
    /// <summary>Registers <see cref="IDispatcher"/>. Calling it more than once is safe.</summary>
    public static IServiceCollection AddDispatcher(this IServiceCollection services)
    {
        services.TryAddScoped<IDispatcher, Dispatcher>();
        return services;
    }

    /// <summary>
    /// Adds a pipeline behavior for every query and command, for example <c>typeof(LoggingBehavior&lt;,&gt;)</c>.
    /// Behaviors run in the order they are added: the first one added is the outermost.
    /// </summary>
    public static IServiceCollection AddPipelineBehavior(this IServiceCollection services, Type openBehaviorType)
    {
        ArgumentNullException.ThrowIfNull(openBehaviorType);
        if (!openBehaviorType.IsGenericTypeDefinition || !Implements(openBehaviorType, typeof(IPipelineBehavior<,>)))
        {
            throw new ArgumentException(
                $"{openBehaviorType.Name} must be an open generic type that implements IPipelineBehavior<,>.", nameof(openBehaviorType));
        }

        services.AddScoped(typeof(IPipelineBehavior<,>), openBehaviorType);
        return services;
    }

    /// <summary>
    /// Registers every query and command handler found in <paramref name="assembly"/> (internal ones too) as scoped services.
    /// </summary>
    /// <remarks>
    /// Each message must have exactly one handler, so a second handler for the same message throws at startup
    /// instead of silently replacing the first. Scanning the same assembly twice is safe.
    /// </remarks>
    public static IServiceCollection AddHandlersFrom(this IServiceCollection services, Assembly assembly)
    {
        foreach (var (service, implementation) in FindImplementations(assembly, typeof(IQueryHandler<,>), typeof(ICommandHandler<,>)))
        {
            var existing = services.FirstOrDefault(descriptor => descriptor.ServiceType == service);
            if (existing is null)
            {
                services.AddScoped(service, implementation);
            }
            else if (existing.ImplementationType != implementation)
            {
                throw new InvalidOperationException(
                    $"{service.GenericTypeArguments[0].Name} already has a handler, so {implementation.FullName} " +
                    "would be a second one. Each query or command must have exactly one handler.");
            }
        }

        return services;
    }

    /// <summary>
    /// Registers every validator found in <paramref name="assembly"/> (internal ones too) as scoped services.
    /// A message can have several validators. Scanning the same assembly twice is safe.
    /// </summary>
    public static IServiceCollection AddValidatorsFrom(this IServiceCollection services, Assembly assembly)
    {
        foreach (var (service, implementation) in FindImplementations(assembly, typeof(IValidator<>)))
        {
            services.TryAddEnumerable(ServiceDescriptor.Scoped(service, implementation));
        }

        return services;
    }

    /// <summary>Finds every concrete class in the assembly and each of the given open generic interfaces it implements.</summary>
    private static IEnumerable<(Type Service, Type Implementation)> FindImplementations(Assembly assembly, params Type[] openInterfaces)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        return
            from type in assembly.GetTypes()
            where type is { IsClass: true, IsAbstract: false, IsGenericTypeDefinition: false }
            from service in type.GetInterfaces()
            where service.IsGenericType && openInterfaces.Contains(service.GetGenericTypeDefinition())
            select (service, type);
    }

    private static bool Implements(Type type, Type openInterface) =>
        type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == openInterface);
}
