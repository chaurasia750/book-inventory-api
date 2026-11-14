using Microsoft.EntityFrameworkCore;
using BookInventory.Domain.Iam;

namespace BookInventory.Infrastructure.Persistence;

/// <summary>
/// The one deliberate escape from the hidden-system-account filter of R43. Every other query must not use it.
/// </summary>
public static class AppUserQueryExtensions
{
    /// <summary>
    /// R43: shows the hidden system account again. Only the login lookup (which must be able to find it) and the startup
    /// seeder (which creates, finds and rotates it) may call this. A list, a search, a count, a dropdown or an export
    /// never may, or the account would stop being invisible.
    /// </summary>
    public static IQueryable<AppUser> IgnoreHiddenAccountFilter(this IQueryable<AppUser> source) =>
        source.IgnoreQueryFilters([AppUser.HiddenAccountFilterKey]);

    /// <summary>
    /// The tracked account with this id, <em>including</em> the hidden system account. One definition for the two stores that
    /// need it (the login lookup by id and the Super Admin management load), so the escape from R43 is written once.
    /// </summary>
    public static Task<AppUser?> FindByIdIncludingHiddenAsync(this IQueryable<AppUser> source, Guid id, CancellationToken cancellationToken = default) =>
        source.IgnoreHiddenAccountFilter().FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    /// <summary>
    /// True once the tables exist. PostgreSQL answers 42P01 (undefined_table) before the first migration has been
    /// applied; that is a normal state during setup, not an error, and both startup seeders skip quietly. A <em>count</em>
    /// is used rather than <c>AnyAsync</c> on purpose: an existing but empty table answers 0, which still proves it is there.
    /// </summary>
    public static async Task<bool> IamSchemaExistsAsync(this IQueryable<AppUser> source, CancellationToken cancellationToken = default)
    {
        try
        {
            await source.CountAsync(cancellationToken);
            return true;
        }
        catch (Exception ex) when (PostgresErrors.IsUndefinedTable(ex))
        {
            return false;
        }
    }
}
