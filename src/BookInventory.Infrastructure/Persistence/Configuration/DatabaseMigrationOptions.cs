namespace BookInventory.Infrastructure.Persistence.Configuration;

/// <summary>
/// Whether the host brings the database schema up to date at startup (spec platform P16), bound from <c>Database:*</c>
/// and validated at startup. The shipped configuration turns it on for every environment (owner decision 2026-10-26:
/// Development and production both migrate on start); an environment that must keep the step with a person sets the
/// flag to <c>false</c>.
/// </summary>
public sealed class DatabaseMigrationOptions
{
    public const string SectionName = "Database";

    /// <summary>
    /// Apply the pending migrations when the host starts, in every environment (owner decision: Development and
    /// production both). Set it to <c>false</c> to keep the migration step with a person instead.
    /// </summary>
    public bool MigrateOnStartup { get; set; }
}
