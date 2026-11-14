using BookInventory.BuildingBlocks.Application.Extensions;
using BookInventory.BuildingBlocks.Application.Messages;
using Microsoft.Extensions.DependencyInjection;
using BookInventory.Application.Company;
using BookInventory.Application.Core;
using BookInventory.Application.Iam;
using BookInventory.Application.Finance;
using BookInventory.Application.Item;
using BookInventory.Application.Price;
using BookInventory.Application.Stock;
using BookInventory.Application.School;
using BookInventory.Application.Vendor;

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
            services.AddMessages(CompanyMessageCatalog.Messages); // company message codes
            services.AddMessages(SchoolMessageCatalog.Messages); // school message codes
            services.AddMessages(VendorMessageCatalog.Messages); // vendor message codes
            services.AddMessages(ItemMessageCatalog.Messages); // item message codes
            services.AddMessages(PriceMessageCatalog.Messages); // price message codes
            services.AddMessages(StockMessageCatalog.Messages); // stock message codes
            services.AddMessages(FinanceMessageCatalog.Messages); // finance message codes

            return services;
        }
    }
}
