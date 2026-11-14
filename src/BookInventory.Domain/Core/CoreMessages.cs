namespace BookInventory.Domain.Core;

/// <summary>
/// Every message code of the <c>core</c> module (spec R55 of iam applies to all modules). The domain throws these codes;
/// the human text lives in <c>BookInventory.Application.Core.CoreMessageCatalog</c>. Codes are stable API surface.
/// </summary>
public static class CoreMessages
{
    public const string TemplateNotFound = "core.notification.template_not_found";
    public const string InvalidRecipient = "core.notification.invalid_recipient";
    public const string InvalidTemplate = "core.notification.invalid_template";
    public const string InvalidTransition = "core.notification.invalid_transition";
    public const string CannotRequeueRedacted = "core.notification.cannot_requeue_redacted";
    public const string NotFound = "core.notification.not_found";
}
