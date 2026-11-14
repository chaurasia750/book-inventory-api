using System.Net;

namespace BookInventory.Application.Iam.Authentication;

/// <summary>
/// What the Application layer may know about the hidden system account's configuration (R46-R50); the options class
/// and the secret stay in Infrastructure. The one rule it carries is <see cref="Admits"/>, the switch and the address
/// allow-list of R46/R47.
/// </summary>
public sealed record SystemAccountSettings(
    bool Enabled,
    IReadOnlyList<IPAddress> AllowedIps,
    string? AlertEmail,
    bool AllowFinancialActions)
{
    /// <summary>
    /// R46/R47: whether a login as the system account from <paramref name="clientIp"/> may go on. A refusal is answered
    /// exactly like an unknown user, so the account never reveals that it exists.
    /// </summary>
    public bool Admits(string? clientIp)
    {
        if (!Enabled)
            return false;
        if (AllowedIps.Count == 0)
            return true;

        return IPAddress.TryParse(clientIp, out var address)
            && AllowedIps.Contains(address.IsIPv4MappedToIPv6 ? address.MapToIPv4() : address);
    }
}
