using BookInventory.BuildingBlocks.Application.Attributes;
using Microsoft.Extensions.DependencyInjection;
using BookInventory.Application.Core;
using BookInventory.Application.Iam.Authentication;
using BookInventory.Domain.Core;

namespace BookInventory.Application.Iam.Passwords;

/// <summary>
/// The default templates <c>iam</c> owns (spec core N13): the password reset mail of R17. It is marked sensitive, so the
/// link is removed from the database once the mail is delivered or given up (core N9). Only a starting text - once it is
/// in the database an operator may reword it, and the seeder will not overwrite that.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped, typeof(INotificationTemplateSource))]
public sealed class IamTemplateSource : INotificationTemplateSource
{
    public IReadOnlyList<TemplateContent> Templates { get; } =
    [
        new TemplateContent(
            PasswordResetLinkIssuer.TemplateCode,
            "Password reset link",
            Subject: "Reset your password",
            Body: """
                <p>Hello {{ fullName }},</p>
                <p>We received a request to reset the password of your account. Use the button below to choose a new one.
                The link works once and expires in {{ expiresInMinutes }} minutes.</p>
                <p style="margin:24px 0;"><a href="{{ resetLink }}" style="background:#1d4ed8;color:#ffffff;padding:12px 20px;border-radius:4px;text-decoration:none;">Reset password</a></p>
                <p>If the button does not work, copy this address into your browser:<br>{{ resetLink }}</p>
                <p>If you did not ask for this, you can ignore this email; your password stays as it is.</p>
                """,
            IsHtml: true,
            IsSensitive: true,
            VariablesHint: "fullName, resetLink, expiresInMinutes"),
        new TemplateContent(
            SystemAccountLoginAlert.TemplateCode,
            "System account sign-in alert",
            Subject: "The system account signed in",
            Body: """
                <p>The hidden system account signed in.</p>
                <p>Time (UTC): {{ signedInAt }}<br>Address: {{ ipAddress }}<br>Browser: {{ userAgent }}</p>
                <p>If nobody on the team expected this, rotate the system account secret and review the audit trail.</p>
                """,
            IsHtml: true,
            IsSensitive: false,
            VariablesHint: "signedInAt, ipAddress, userAgent")
    ];
}
