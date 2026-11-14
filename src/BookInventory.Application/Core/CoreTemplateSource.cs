using BookInventory.BuildingBlocks.Application.Attributes;
using Microsoft.Extensions.DependencyInjection;
using BookInventory.Domain.Core;

namespace BookInventory.Application.Core;

/// <summary>
/// The default templates <c>core</c> owns (spec N13): the master layout every HTML email is wrapped in. The text is only
/// a starting point - once it is in the database an operator may restyle it, and the seeder will not overwrite that.
/// The company values come from <c>MasterLayout:*</c>; <c>content</c> and <c>subject</c> are filled per email.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped, typeof(INotificationTemplateSource))]
public sealed class CoreTemplateSource : INotificationTemplateSource
{
    private const string Layout = """
        <!DOCTYPE html>
        <html>
        <head><meta charset="utf-8"><title>{{ subject }}</title></head>
        <body style="margin:0;padding:0;background:#f4f5f7;font-family:Arial,Helvetica,sans-serif;color:#1f2933;">
          <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="background:#f4f5f7;padding:24px 0;">
            <tr><td align="center">
              <table role="presentation" width="600" cellpadding="0" cellspacing="0" style="background:#ffffff;border-radius:6px;">
                <tr><td style="padding:24px 32px;border-bottom:1px solid #e4e7eb;">
                  {{ if companyLogo != "" }}<img src="{{ companyLogo }}" alt="{{ companyName }}" height="40" style="display:block;">{{ else }}<strong style="font-size:18px;">{{ companyName }}</strong>{{ end }}
                </td></tr>
                <tr><td style="padding:32px;font-size:15px;line-height:1.5;">{{ content }}</td></tr>
                <tr><td style="padding:20px 32px;background:#f9fafb;border-top:1px solid #e4e7eb;font-size:12px;color:#616e7c;">
                  {{ companyName }}{{ if companyAddress != "" }} &middot; {{ companyAddress }}{{ end }}<br>
                  {{ if supportEmail != "" }}{{ supportEmail }}{{ end }}{{ if supportPhone != "" }} &middot; {{ supportPhone }}{{ end }}
                </td></tr>
              </table>
            </td></tr>
          </table>
        </body>
        </html>
        """;

    public IReadOnlyList<TemplateContent> Templates { get; } =
    [
        new TemplateContent(
            NotificationComposer.MasterLayoutCode,
            "Master email layout",
            Subject: string.Empty,
            Body: Layout,
            IsHtml: true,
            IsSensitive: false,
            VariablesHint: "content, subject, companyName, companyLogo, companyWebsite, companyAddress, supportEmail, supportPhone")
    ];
}
