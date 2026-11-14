using BookInventory.BuildingBlocks.Application.Extensions;
using BookInventory.BuildingBlocks.Persistence.Postgres;
using BookInventory.Infrastructure.Core;
using BookInventory.Infrastructure.Persistence;

namespace BookInventory.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("Default")
                ?? throw new InvalidOperationException("ConnectionStrings:Default is not configured.");

            services.AddSingleton(TimeProvider.System);
            services.AddPostgresPersistence<AppDbContext>(
                connectionString,
                npgsql => npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName));

            // Repository implementations, external service clients, ... marked [AutoRegister] in this assembly.
            services.AddAutoRegisteredServices(typeof(DependencyInjection).Assembly);
            services.AddNotificationSender();

            return services;
        }
    }
}
