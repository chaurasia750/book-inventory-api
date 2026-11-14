using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace BookInventory.BuildingBlocks.Logger
{
    public static class LoggerConfigurationExtension
    {
        /// <summary>
        /// Configures Serilog from, in increasing priority: the packaged <paramref name="settingsFile"/> defaults,
        /// then the host configuration (appsettings.json, environment variables), so any service can override the
        /// sinks or levels without editing this library. The <c>Serilog</c> section is used in both.
        /// </summary>
        public static IHostBuilder AddLogger(this IHostBuilder host, string settingsFile = "serilog.settings.json")
        {
            return host.UseSerilog((context, services, logger) =>
            {
                var defaults = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile(settingsFile, optional: true, reloadOnChange: false)
                    .AddConfiguration(context.Configuration)
                    .Build();

                logger
                    .ReadFrom.Configuration(defaults)
                    .ReadFrom.Services(services)
                    .Enrich.FromLogContext()
                    .Enrich.WithProperty("Application", context.HostingEnvironment.ApplicationName)
                    .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName);
            });
        }

        /// <summary>One structured log line per HTTP request (method, path, status, elapsed) instead of the noisy framework logs.</summary>
        public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app) =>
            app.UseSerilogRequestLogging();
    }
}
