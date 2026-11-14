using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using MailKit.Security;

namespace BookInventory.Infrastructure.Core.Configuration;

/// <summary>
/// The SMTP provider (spec core N11, N12), from <c>Email:*</c>. An empty <see cref="Host"/> is a valid setting: it means
/// "no provider yet" and selects the logging sender. Once a host is set everything else is validated at startup, so a
/// half-configured provider stops the application instead of failing every mail later. The password comes from a
/// secret store (user-secrets or the environment), never from a committed file.
/// </summary>
public sealed class EmailOptions : IValidatableObject
{
    public const string SectionName = "Email";

    public string Host { get; set; } = string.Empty;

    public int Port { get; set; } = 587;

    /// <summary><c>StartTls</c> (587), <c>SslOnConnect</c> (465), <c>Auto</c> or <c>None</c>.</summary>
    public string Security { get; set; } = nameof(SecureSocketOptions.StartTls);

    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string FromEmail { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>Development only: the logging sender writes the rendered email to the log (N12).</summary>
    public bool LogBody { get; set; }

    public bool HasProvider => !string.IsNullOrWhiteSpace(Host);

    public SecureSocketOptions SocketOptions => Enum.Parse<SecureSocketOptions>(Security, ignoreCase: true);

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!HasProvider)
            yield break;

        if (Port is < 1 or > 65535)
            yield return new ValidationResult("Email:Port must be between 1 and 65535.", [nameof(Port)]);
        if (!MailAddress.TryCreate(FromEmail, out _))
            yield return new ValidationResult("Email:FromEmail must be a valid address when Email:Host is set.", [nameof(FromEmail)]);
        if (!Enum.TryParse<SecureSocketOptions>(Security, ignoreCase: true, out _))
            yield return new ValidationResult("Email:Security must be None, Auto, SslOnConnect or StartTls.", [nameof(Security)]);
        if (TimeoutSeconds is < 1 or > 300)
            yield return new ValidationResult("Email:TimeoutSeconds must be between 1 and 300.", [nameof(TimeoutSeconds)]);
        if (!string.IsNullOrWhiteSpace(Username) && string.IsNullOrEmpty(Password))
            yield return new ValidationResult("Email:Password is required when Email:Username is set (use user-secrets).", [nameof(Password)]);
    }
}
