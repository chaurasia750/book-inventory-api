using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using BookInventory.Application.Core;
using BookInventory.Infrastructure.Core.Configuration;

namespace BookInventory.Infrastructure.Core;

public static class CoreInfrastructureExtensions
{
    /// <summary>
    /// Spec core N11/N12: the sender is chosen by configuration and by nothing else - SMTP when <c>Email:Host</c> is set,
    /// the logging sender otherwise - so plugging a provider in changes no module.
    /// </summary>
    public static IServiceCollection AddNotificationSender(this IServiceCollection services)
    {
        services.TryAddScoped<INotificationSender>(provider =>
            provider.GetRequiredService<IOptions<EmailOptions>>().Value.HasProvider
                ? ActivatorUtilities.CreateInstance<MailKitEmailSender>(provider)
                : ActivatorUtilities.CreateInstance<LoggingEmailSender>(provider));

        return services;
    }
}
