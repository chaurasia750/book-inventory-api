using BookInventory.BuildingBlocks.Application.Messages;
using BookInventory.BuildingBlocks.Configuration.Exceptions;
using BookInventory.BuildingBlocks.Domain;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BookInventory.BuildingBlocks.Configuration.Extensions
{
    /// <summary>One-call host setup. Typical <c>Program.cs</c>:
    /// <code>
    /// builder.Host.AddLogger();
    /// builder.Services.AddApiBuildingBlocks(builder.Configuration);
    /// builder.Services.AddPostgresPersistence&lt;AppDbContext&gt;(connectionString);
    /// var app = builder.Build();
    /// app.UseApiBuildingBlocks(builder.Configuration);
    /// app.MapApiBuildingBlocks();
    /// app.Run();
    /// </code>
    /// </summary>
    public static class ApiBuildingBlocksExtensions
    {
        public static IServiceCollection AddApiBuildingBlocks(this IServiceCollection services, IConfiguration configuration)
        {
            services.TryAddSingleton<IMessageCatalog, MessageCatalog>();
            services.AddProblemDetails();
            services.AddExceptionHandler<GlobalExceptionHandler>();
            services.AddApiVersion();
            services.AddCorsPolicy(configuration);
            services.AddJwtAuthentication(configuration);
            services.AddMvcWithFilters();
            services.AddOpenApi();
            services.AddHealthChecks();
            return services;
        }

        public static IApplicationBuilder UseApiBuildingBlocks(this IApplicationBuilder app, IConfiguration configuration)
        {
            app.UseExceptionHandler();
            app.UseStatusCodePages();
            app.UseCorsPolicy(configuration);
            app.UseAuthentication();
            app.UseAuthorization();
            return app;
        }

        /// <summary>Maps controllers, <c>/health</c>, and (Development only) the OpenAPI document at <c>/openapi/v1.json</c>.</summary>
        public static WebApplication MapApiBuildingBlocks(this WebApplication app)
        {
            app.MapControllers();
            app.MapHealthChecks("/health").AllowAnonymous();

            if (app.Environment.IsDevelopment())
                app.MapOpenApi().AllowAnonymous();

            return app;
        }
    }
}
