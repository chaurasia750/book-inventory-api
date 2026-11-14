using BookInventory.BuildingBlocks.Domain;
using BookInventory.BuildingBlocks.Domain.Exceptions;

namespace BookInventory.Domain.Core;

/// <summary>
/// A named, editable text with <c>{{ variables }}</c>, identified by a stable code such as <c>PASSWORD_RESET</c>
/// (spec core N3, N13). The database copy is what the worker renders, so the wording can change without a release.
/// </summary>
public class NotificationTemplate : AggregateRoot
{
    public const int CodeMaxLength = 100;

    private NotificationTemplate()
    {
        Code = string.Empty;
        Name = string.Empty;
        Subject = string.Empty;
        Body = string.Empty;
        LanguageCode = "en";
    }

    public string Code { get; private set; }

    public string Name { get; private set; }

    public NotificationChannel Channel { get; private set; }

    public string Subject { get; private set; }

    public string Body { get; private set; }

    public bool IsHtml { get; private set; }

    /// <summary>N9: a notification made from this template loses its variables once it is delivered or given up.</summary>
    public bool IsSensitive { get; private set; }

    public string LanguageCode { get; private set; }

    public bool IsActive { get; private set; }

    /// <summary>Documents the variables the text expects, for whoever edits it later.</summary>
    public string? VariablesHint { get; private set; }

    public static NotificationTemplate Create(TemplateContent content)
    {
        var template = new NotificationTemplate { Channel = NotificationChannel.Email, IsActive = true };
        template.Apply(content);
        return template;
    }

    /// <summary>Changes the wording only; the code is the template's identity and never changes.</summary>
    public void Update(string subject, string body, bool isHtml)
    {
        Subject = subject ?? string.Empty;
        Body = RequireText(body);
        IsHtml = isHtml;
    }

    public void SetActive(bool isActive) => IsActive = isActive;

    private void Apply(TemplateContent content)
    {
        var code = content.Code?.Trim().ToUpperInvariant();
        if (string.IsNullOrEmpty(code) || code.Length > CodeMaxLength)
            throw new ValidationFailureException(CoreMessages.InvalidTemplate);

        Code = code;
        Name = string.IsNullOrWhiteSpace(content.Name) ? code : content.Name.Trim();
        Subject = content.Subject ?? string.Empty;
        Body = RequireText(content.Body);
        IsHtml = content.IsHtml;
        IsSensitive = content.IsSensitive;
        VariablesHint = content.VariablesHint;
    }

    private static string RequireText(string? body)
    {
        if (string.IsNullOrWhiteSpace(body))
            throw new ValidationFailureException(CoreMessages.InvalidTemplate);
        return body;
    }
}
