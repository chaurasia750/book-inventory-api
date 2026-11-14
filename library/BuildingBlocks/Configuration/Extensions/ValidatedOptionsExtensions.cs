using Microsoft.Extensions.Options;

namespace BookInventory.BuildingBlocks.Configuration.Extensions
{
    public static class ValidatedOptionsExtensions
    {
        /// <summary>
        /// Binds <paramref name="section"/> to <typeparamref name="T"/>, validates it with data annotations and makes the
        /// application fail at startup (not at first use) when a value is missing or out of range.
        /// </summary>
        public static OptionsBuilder<T> AddValidatedOptions<T>(this IServiceCollection services, IConfiguration configuration, string section)
            where T : class =>
            services.AddOptions<T>()
                .Bind(configuration.GetSection(section))
                .ValidateDataAnnotations()
                .ValidateOnStart();
    }
}
