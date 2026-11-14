using System.Text.Json;
using System.Text.Json.Serialization;
using BookInventory.BuildingBlocks.Configuration.Filters;

namespace BookInventory.BuildingBlocks.Configuration.Extensions
{
    public static class FilterExtension
    {
        /// <summary>The prefix every controller route gets (<c>api/v1/...</c>).</summary>
        public const string ApiRoutePrefix = "api/v{version:apiVersion}";

        /// <summary>
        /// Controllers with the versioned route prefix and the per-write-request transaction filter;
        /// enums serialize as strings.
        /// </summary>
        public static IMvcBuilder AddMvcWithFilters(this IServiceCollection services)
        {
            services.AddScoped<TransactionFilter>();

            return services.AddControllers(options =>
                {
                    options.Conventions.Add(new RoutePrefixConvention(ApiRoutePrefix));
                    options.Filters.Add<TransactionFilter>();
                })
                .AddJsonOptions(options =>
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseUpper, allowIntegerValues: false)));
        }
    }
}
