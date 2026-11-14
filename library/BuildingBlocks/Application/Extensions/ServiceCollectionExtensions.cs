using System.Reflection;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Application.EventDispatcher;
using BookInventory.BuildingBlocks.Application.Messages;
using BookInventory.BuildingBlocks.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BookInventory.BuildingBlocks.Application.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers the domain event dispatcher and every <see cref="IDomainEventHandler{TEvent}"/> /
        /// <see cref="IExternalEventHandler{TEvent}"/> found in the given assemblies (scoped).
        /// </summary>
        public static IServiceCollection AddApplicationBuildingBlocks(this IServiceCollection services, params Assembly[] assemblies)
        {
            services.TryAddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
            services.TryAddSingleton<IMessageCatalog, MessageCatalog>();

            foreach (var type in assemblies.SelectMany(GetLoadableTypes).Where(IsConcrete))
            {
                foreach (var contract in type.GetInterfaces().Where(IsEventHandlerContract))
                {
                    services.TryAddEnumerable(ServiceDescriptor.Scoped(contract, type));
                }
            }

            return services;
        }

        /// <summary>
        /// Registers every class marked with <see cref="AutoRegisterAttribute"/> in the given assemblies.
        /// Pass the assemblies explicitly (e.g. <c>typeof(SomeTypeInModule).Assembly</c>) so behaviour never
        /// depends on which assemblies happen to be loaded at startup.
        /// </summary>
        public static IServiceCollection AddAutoRegisteredServices(this IServiceCollection services, params Assembly[] assemblies)
        {
            ArgumentNullException.ThrowIfNull(assemblies);

            foreach (var type in assemblies.Distinct().SelectMany(GetLoadableTypes).Where(IsConcrete))
            {
                var attribute = type.GetCustomAttribute<AutoRegisterAttribute>(inherit: false);
                if (attribute is null)
                    continue;

                foreach (var serviceType in ResolveServiceTypes(type, attribute))
                {
                    services.Add(new ServiceDescriptor(serviceType, type, attribute.ServiceLifetime));
                }
            }

            return services;
        }

        private static Type[] ResolveServiceTypes(Type implementation, AutoRegisterAttribute attribute)
        {
            if (implementation.GetConstructors().Length == 0)
                throw new InvalidOperationException(
                    $"'{implementation.FullName}' is marked [AutoRegister] but has no public constructor.");

            if (attribute.IncludeInterfaces)
            {
                var interfaces = implementation.GetInterfaces()
                    .Where(i => i != typeof(IDisposable) && i != typeof(IAsyncDisposable))
                    .ToArray();
                return interfaces.Length > 0 ? interfaces : [implementation];
            }

            if (attribute.ServiceType is { } serviceType)
            {
                if (!serviceType.IsAssignableFrom(implementation))
                    throw new InvalidOperationException(
                        $"'{implementation.FullName}' does not implement '{serviceType.FullName}'.");
                return [serviceType];
            }

            return [implementation];
        }

        private static bool IsConcrete(Type type) => type is { IsClass: true, IsAbstract: false, IsGenericTypeDefinition: false };

        private static bool IsEventHandlerContract(Type type) =>
            type.IsGenericType &&
            type.GetGenericTypeDefinition() is var definition &&
            (definition == typeof(IDomainEventHandler<>) || definition == typeof(IExternalEventHandler<>));

        private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(t => t is not null)!;
            }
        }
    }
}
