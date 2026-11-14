using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using BookInventory.Application.Core;
using BookInventory.Domain.Core;
using BookInventory.Infrastructure.Core.Configuration;

namespace BookInventory.Infrastructure.Core;

/// <summary>
/// The sender used while <c>Email:Host</c> is empty (spec core N12): it walks the same steps as a real provider and
/// reports success with <c>logged only, no provider configured</c>, so forgot-password works end to end in development
/// and nothing changes when SMTP is configured. It writes the rendered email only when <c>Email:LogBody</c> is on
/// (Development settings), because a reset link in a log is a credential. This must never be the production wiring; the
/// release checklist lists it.
/// </summary>
public sealed class LoggingEmailSender(IOptions<EmailOptions> options, ILogger<LoggingEmailSender> logger) : INotificationSender
{
    public const string Response = "logged only, no provider configured";

    public Task<SendOutcome> SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("No email provider is configured; email to {Recipient} was logged only.", EmailMask.Mask(message.To));

        if (options.Value.LogBody)
            logger.LogInformation("DEVELOPMENT EMAIL to {Recipient}, subject '{Subject}':{NewLine}{Body}", message.To, message.Subject, Environment.NewLine, message.Body);

        return Task.FromResult(new SendOutcome(true, null, Response, null));
    }
}
