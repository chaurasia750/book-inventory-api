using BookInventory.Domain.Core;

namespace BookInventory.Application.Core;

/// <summary>
/// Delivers one email (spec N11, N12). The implementation is chosen by configuration - SMTP when <c>Email:Host</c> is
/// set, a logging sender otherwise - and reports the result instead of throwing, so a refusal from the mail server is
/// an ordinary outcome the retry policy handles.
/// </summary>
public interface INotificationSender
{
    Task<SendOutcome> SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
}
