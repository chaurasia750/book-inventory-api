using System.Net;
using System.Text.Json;
using BookInventory.BuildingBlocks.Application.Attributes;
using Microsoft.Extensions.DependencyInjection;
using BookInventory.Domain.Core;

namespace BookInventory.Application.Core;

/// <summary>
/// Spec core N3, N4: turns a recorded notification into the email that goes out - the content template rendered with the
/// stored variables, wrapped in the master layout when the template is HTML and the layout exists. It runs when the
/// worker sends, so a template edited after queueing is used; the rendered text is never stored (N9).
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class NotificationComposer(
    INotificationStore store,
    ITemplateRenderer renderer,
    NotificationSettings settings)
{
    public const string MasterLayoutCode = "MASTER_EMAIL_LAYOUT";

    public async Task<EmailMessage> ComposeAsync(Notification notification, CancellationToken cancellationToken)
    {
        var template = await store.FindTemplateAsync(notification.TemplateCode, cancellationToken);
        if (template is not { IsActive: true })
            throw new TemplateRenderingException($"Template {notification.TemplateCode} was not found or is switched off.");
        if (notification.IsRedacted)
            throw new TemplateRenderingException("The variables of this notification have been removed.");

        // An HTML body gets HTML-encoded values (a name like "A<b>" must not become markup); the subject is plain text.
        var subject = await renderer.RenderAsync(template.Subject, BuildModel(notification.Variables, htmlEncode: false), cancellationToken);
        var model = BuildModel(notification.Variables, htmlEncode: template.IsHtml);
        var content = await renderer.RenderAsync(template.Body, model, cancellationToken);

        var layout = template.IsHtml ? await store.FindTemplateAsync(MasterLayoutCode, cancellationToken) : null;
        if (layout is not { IsActive: true })
            return new EmailMessage(notification.Recipient, subject, content, template.IsHtml);

        var layoutModel = new Dictionary<string, object?>(model) { ["content"] = content, ["subject"] = WebUtility.HtmlEncode(subject) };
        var html = await renderer.RenderAsync(layout.Body, layoutModel, cancellationToken);
        return new EmailMessage(notification.Recipient, subject, html, IsHtml: true);
    }

    /// <summary>The caller's variables plus the company details of the master layout.</summary>
    private Dictionary<string, object?> BuildModel(string variablesJson, bool htmlEncode)
    {
        var model = new Dictionary<string, object?>
        {
            ["companyName"] = Encode(settings.Layout.CompanyName, htmlEncode),
            ["companyLogo"] = Encode(settings.Layout.CompanyLogo, htmlEncode),
            ["companyWebsite"] = Encode(settings.Layout.CompanyWebsite, htmlEncode),
            ["companyAddress"] = Encode(settings.Layout.CompanyAddress, htmlEncode),
            ["supportEmail"] = Encode(settings.Layout.SupportEmail, htmlEncode),
            ["supportPhone"] = Encode(settings.Layout.SupportPhone, htmlEncode)
        };

        var variables = JsonSerializer.Deserialize<Dictionary<string, string>>(variablesJson) ?? [];
        foreach (var (key, value) in variables)
            model[key] = Encode(value, htmlEncode);

        return model;
    }

    private static string Encode(string value, bool htmlEncode) => htmlEncode ? WebUtility.HtmlEncode(value) : value;
}
