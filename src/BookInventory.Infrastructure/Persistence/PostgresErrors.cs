using Npgsql;

namespace BookInventory.Infrastructure.Persistence;

/// <summary>Reads PostgreSQL error codes off an exception, so the startup seeders share one definition of "not migrated yet".</summary>
public static class PostgresErrors
{
    /// <summary>
    /// True for 42P01 (undefined_table): the migration that creates the table has not been applied, which is a normal
    /// state during setup and not an error. Walks the exception chain, because a provider may wrap the Npgsql error.
    /// </summary>
    public static bool IsUndefinedTable(Exception exception)
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            if (current is PostgresException { SqlState: PostgresErrorCodes.UndefinedTable })
                return true;
        }

        return false;
    }
}
