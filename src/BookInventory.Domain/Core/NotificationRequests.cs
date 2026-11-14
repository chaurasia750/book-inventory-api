namespace BookInventory.Domain.Core;

/// <summary>What a module asks for when it wants somebody told (spec core N2): the template, the person, the values and the key that makes the request repeatable.</summary>
public sealed record NewNotification(
    string TemplateCode,
    string Recipient,
    string Variables,
    string IdempotencyKey,
    string? CorrelationId = null);

/// <summary>The rules a notification is created under, copied from its template and the configuration.</summary>
public sealed record NotificationPolicy(bool IsSensitive, int MaxRetry, NotificationPriority Priority = NotificationPriority.Normal);

/// <summary>The outcome of one send attempt, as the sender reports it.</summary>
public sealed record SendOutcome(bool Success, string? ProviderMessageId, string? Response, string? Error);

/// <summary>The content of a template (spec core section 4).</summary>
public sealed record TemplateContent(string Code, string Name, string Subject, string Body, bool IsHtml, bool IsSensitive, string? VariablesHint = null);
