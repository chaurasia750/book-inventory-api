namespace BookInventory.Domain.Iam;

/// <summary>
/// User names nobody may register for themselves, because they are either the hidden system account (R51) or would
/// be mistaken for support staff. Checked in normalised (lower-case) form.
/// </summary>
public static class ReservedUserNames
{
    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
    {
        "system", "sys", "sysadmin", "admin", "administrator", "root", "superuser", "su", "sa",
        "postgres", "api", "app", "web", "support", "help", "info", "contact", "abuse",
        "hostmaster", "postmaster", "webmaster", "noreply", "no-reply"
    };

    public static bool IsReserved(string? normalizedUserName) =>
        normalizedUserName is not null && All.Contains(normalizedUserName);
}
