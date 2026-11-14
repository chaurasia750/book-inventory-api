using System.ComponentModel.DataAnnotations;
using System.Net;
using BookInventory.Domain.Iam;

namespace BookInventory.Infrastructure.Iam.Configuration;

/// <summary>
/// The hidden system account of R42-R52, from <c>SystemAccount:*</c>. The password must come from user-secrets, an
/// environment variable or another secret store, never from a committed file (R51): without it nothing is seeded.
/// Enabled by default in every environment (R46, owner decision); <see cref="Enabled"/> switches it off without a code
/// change, and <see cref="AllowedIps"/>, <see cref="AlertEmail"/> and <see cref="AllowFinancialActions"/> are the
/// optional controls of R47, R50 and R49.
/// </summary>
public sealed class SystemAccountOptions : IValidatableObject
{
    public const string SectionName = "SystemAccount";

    public bool Enabled { get; set; } = true;

    public string UserName { get; set; } = "system";

    public string Password { get; set; } = string.Empty;

    /// <summary>R47: empty means any address; otherwise a login is accepted only from these addresses.</summary>
    public List<string> AllowedIps { get; set; } = [];

    /// <summary>R50: when set, every successful login of the account is mailed here through the core outbox.</summary>
    public string? AlertEmail { get; set; }

    /// <summary>R49: <c>false</c> makes the <c>FinancialAction</c> policy refuse the account.</summary>
    public bool AllowFinancialActions { get; set; } = true;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!IdentityNormalizer.IsValidUserName(IdentityNormalizer.NormalizeUserName(UserName)))
            yield return new ValidationResult($"{SectionName}:UserName is not a valid user name.", [nameof(UserName)]);

        foreach (var ip in AllowedIps.Where(ip => !IPAddress.TryParse(ip, out _)))
            yield return new ValidationResult($"{SectionName}:AllowedIps contains '{ip}', which is not an IP address.", [nameof(AllowedIps)]);

        if (!string.IsNullOrWhiteSpace(AlertEmail) && !IdentityNormalizer.IsValidEmail(IdentityNormalizer.NormalizeEmail(AlertEmail)))
            yield return new ValidationResult($"{SectionName}:AlertEmail is not an e-mail address.", [nameof(AlertEmail)]);
    }
}
