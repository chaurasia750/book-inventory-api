using BookInventory.BuildingBlocks.Application.Extensions;
using BookInventory.BuildingBlocks.Application.Messages;
using Microsoft.Extensions.DependencyInjection;
using BookInventory.Application.Core;
using BookInventory.Application.Iam;

namespace BookInventory.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            var assembly = typeof(DependencyInjection).Assembly;

            services.AddApplicationBuildingBlocks(assembly);   // domain / external event handlers
            services.AddAutoRegisteredServices(assembly);      // application services marked [AutoRegister]
            services.AddMessages(IamMessageCatalog.Messages); // R55: human text for the iam message codes
            services.AddMessages(CoreMessageCatalog.Messages); // core notification message codes

            return services;
        }
    }
}
