using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.BuildingBlocks.Application.Attributes
{
    /// <summary>
    /// Marks a class for automatic DI registration (see <c>AddAutoRegisteredServices</c>).
    /// <code>
    /// [AutoRegister]                                              // scoped, as itself
    /// [AutoRegister(ServiceLifetime.Singleton, typeof(IFoo))]    // against one interface
    /// [AutoRegister(ServiceLifetime.Scoped, IncludeInterfaces = true)]
    /// </code>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class AutoRegisterAttribute(
        ServiceLifetime serviceLifetime = ServiceLifetime.Scoped,
        Type? serviceType = null) : Attribute
    {
        public ServiceLifetime ServiceLifetime { get; } = serviceLifetime;

        /// <summary>Service type to register against; null registers the class as itself.</summary>
        public Type? ServiceType { get; } = serviceType;

        /// <summary>Register against every implemented interface. Takes precedence over <see cref="ServiceType"/>.</summary>
        public bool IncludeInterfaces { get; init; }
    }
}
