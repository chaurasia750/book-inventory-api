using Asp.Versioning;

namespace BookInventory.BuildingBlocks.Configuration.Extensions
{
    public static class ApiVersioningExtension
    {
        /// <summary>Version 1.0 by default; clients select a version with <c>/api/v2/...</c> or the <c>X-Api-Version</c> header.</summary>
        public static IServiceCollection AddApiVersion(this IServiceCollection services)
        {
            services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = ApiVersionReader.Combine(
                    new UrlSegmentApiVersionReader(),
                    new HeaderApiVersionReader("X-Api-Version"));
            }).AddMvc();

            return services;
        }
    }
}
