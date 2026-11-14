using BookInventory.BuildingBlocks.Persistence.Contracts;
using BookInventory.BuildingBlocks.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace BookInventory.BuildingBlocks.Persistence.Postgres
{
    public static class PostgresServiceCollectionExtensions
    {
        /// <summary>
        /// Registers <typeparamref name="TContext"/> against PostgreSQL with snake_case table, column, key, index and
        /// constraint names (<c>UserProfile.FirstName</c> becomes <c>user_profile.first_name</c>), plus
        /// <see cref="IUnitOfWork"/> and open-generic <see cref="IRepository{T}"/>, all scoped.
        /// </summary>
        /// <param name="configureNpgsql">Extra Npgsql options (schema history table, command timeout, ...).</param>
        /// <param name="configure">Extra DbContext options (interceptors, logging, ...).</param>
        public static IServiceCollection AddPostgresPersistence<TContext>(
            this IServiceCollection services,
            string connectionString,
            Action<NpgsqlDbContextOptionsBuilder>? configureNpgsql = null,
            Action<DbContextOptionsBuilder>? configure = null)
            where TContext : BuildingBlocksDbContext
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

            // A single shared data source: pooled connections, and the place to add type mappings later.
            // Registered through a factory so the container disposes it on shutdown.
            services.AddSingleton(_ => new NpgsqlDataSourceBuilder(connectionString).Build());

            services.AddDbContext<TContext>((sp, options) =>
            {
                var dataSource = sp.GetRequiredService<NpgsqlDataSource>();

                // No EnableRetryOnFailure: the request transaction (UnitOfWork) is user-initiated, which the
                // retrying execution strategy does not allow. Opt in through configureNpgsql if you drop the filter.
                options.UseNpgsql(dataSource, npgsql =>
                {
                    npgsql.MigrationsHistoryTable("__ef_migrations_history");
                    configureNpgsql?.Invoke(npgsql);
                });
                options.UseSnakeCaseNamingConvention();
                configure?.Invoke(options);
            });

            services.AddScoped<DbContext>(sp => sp.GetRequiredService<TContext>());
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            return services;
        }
    }
}
