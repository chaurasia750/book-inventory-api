namespace BookInventory.Infrastructure.Iam.Configuration;

/// <summary>
/// The Super Admin a fresh deployment is seeded with, from <c>Seed:SuperAdmin:*</c>. The password must come from
/// user-secrets, an environment variable or another secret store - never from a committed appsettings file - and the
/// seeder refuses to seed without it. Nothing here is required at startup: once a Super Admin exists the values may be
/// removed again, and only an empty database needs them. The seeded account is a normal, <em>visible</em> Super Admin
/// with <c>must_change_password = true</c>; it is not the hidden system account (R42, which is seeded from
/// <c>SystemAccount:*</c>).
/// </summary>
public sealed class SuperAdminSeedOptions
{
    public const string SectionName = "Seed:SuperAdmin";

    /// <summary>Rejects a reserved name such as <c>system</c>, because the system account owns those (R51).</summary>
    public string UserName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}
