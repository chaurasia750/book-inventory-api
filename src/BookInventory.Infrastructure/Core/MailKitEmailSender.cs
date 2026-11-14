using Microsoft.Extensions.Logging;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using BookInventory.Application.Core;
using BookInventory.Domain.Core;
using BookInventory.Infrastructure.Core.Configuration;

namespace BookInventory.Infrastructure.Core;

/// <summary>
/// Sends one email through the configured SMTP server with MailKit (spec core N11). It never throws for a refused or
/// unreachable server: the reason becomes the outcome, which the notification records and the retry policy acts on. The
/// address is masked in the log and neither the body nor the password is ever written. It is selected instead of the
/// logging sender when <c>Email:Host</c> is set (see <c>CoreInfrastructureExtensions</c>).
/// </summary>
public sealed class MailKitEmailSender(IOptions<EmailOptions> options, ILogger<MailKitEmailSender> logger) : INotificationSender
{
    public async Task<SendOutcome> SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        var settings = options.Value;

        try
        {
            using var client = new SmtpClient { Timeout = settings.TimeoutSeconds * 1000 };
            await client.ConnectAsync(settings.Host, settings.Port, settings.SocketOptions, cancellationToken);
            if (!string.IsNullOrWhiteSpace(settings.Username))
                await client.AuthenticateAsync(settings.Username, settings.Password, cancellationToken);

            var response = await client.SendAsync(Build(message, settings), cancellationToken);
            await client.DisconnectAsync(quit: true, cancellationToken);

            logger.LogInformation("Email sent to {Recipient}.", EmailMask.Mask(message.To));
            return new SendOutcome(true, response, "Accepted by the SMTP server", null);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogWarning("Email to {Recipient} was not sent: {Error}", EmailMask.Mask(message.To), ex.Message);
            return new SendOutcome(false, null, null, ex.Message);
        }
    }

    private static MimeMessage Build(EmailMessage message, EmailOptions settings)
    {
        var mime = new MimeMessage();
        mime.From.Add(new MailboxAddress(settings.DisplayName, settings.FromEmail));
        mime.To.Add(MailboxAddress.Parse(message.To));
        mime.Subject = message.Subject;
        mime.Body = new TextPart(message.IsHtml ? TextFormat.Html : TextFormat.Plain) { Text = message.Body };
        return mime;
    }
}
