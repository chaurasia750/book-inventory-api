namespace BookInventory.BuildingBlocks.Configuration.Extensions
{
    public static class CorsPolicyExtension
    {
        private const string DefaultPolicyName = "CorsPolicy";

        /// <summary>
        /// Configuration: <c>Cors:AllowedOrigins</c> (string array) and optional <c>CorsPolicy</c> (policy name).
        /// With origins configured, only those origins may call the API, with credentials (cookies).
        /// With none configured, CORS is switched off altogether: no CORS header is sent, so browsers only allow
        /// same-origin calls (the API that serves its own UI needs nothing else, and no website can read its answers).
        /// </summary>
        public static IServiceCollection AddCorsPolicy(this IServiceCollection services, IConfiguration configuration)
        {
            var origins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

            services.AddCors(options => options.AddPolicy(PolicyName(configuration), policy =>
            {
                if (origins.Length > 0)
                    policy.WithOrigins(origins).AllowCredentials();

                policy.AllowAnyMethod()
                      .AllowAnyHeader()
                      .WithExposedHeaders("Content-Disposition");
            }));

            return services;
        }

        public static IApplicationBuilder UseCorsPolicy(this IApplicationBuilder app, IConfiguration configuration) =>
            HasOrigins(configuration) ? app.UseCors(PolicyName(configuration)) : app;

        private static bool HasOrigins(IConfiguration configuration) =>
            configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() is { Length: > 0 };

        private static string PolicyName(IConfiguration configuration) =>
            configuration["CorsPolicy"] is { Length: > 0 } name ? name : DefaultPolicyName;
    }
}
