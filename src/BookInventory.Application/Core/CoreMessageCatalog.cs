using BookInventory.Domain.Core;

namespace BookInventory.Application.Core;

/// <summary>The human text of every <see cref="CoreMessages"/> code; the code travels, the text is looked up (R55).</summary>
public static class CoreMessageCatalog
{
    public static IReadOnlyDictionary<string, string> Messages { get; } =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [CoreMessages.TemplateNotFound] = "The message template could not be found or is switched off.",
            [CoreMessages.InvalidRecipient] = "The recipient address is not a valid email address.",
            [CoreMessages.InvalidTemplate] = "The message template is incomplete.",
            [CoreMessages.InvalidTransition] = "The notification is not in a state that allows this change.",
            [CoreMessages.CannotRequeueRedacted] = "This notification held a secret that has been removed, so it cannot be sent again.",
            [CoreMessages.NotFound] = "The notification could not be found.",
            [CoreMessages.AuditCompanyRequired] = "The audit log needs a company."
        };
}
