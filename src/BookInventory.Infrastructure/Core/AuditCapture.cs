using System.Reflection;
using System.Text.Json;
using BookInventory.BuildingBlocks.Domain;
using BookInventory.BuildingBlocks.Persistence.Naming;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using BookInventory.Domain.Core;

namespace BookInventory.Infrastructure.Core;

/// <summary>
/// Spec core A1-A5: turns the changes the change tracker is about to save into audit entries. An added row has only a new
/// value, a deleted one only an old value, an update both and only for the fields that changed (A2); a column marked
/// <see cref="AuditSecretAttribute"/> and an entity marked <see cref="AuditIgnoreAttribute"/> are never recorded (A3,
/// AQ1); the audit columns, the row version and the soft-delete flag are bookkeeping, not changes of the business row.
/// </summary>
internal static class AuditCapture
{
    private static readonly HashSet<string> Bookkeeping =
    [
        nameof(DomainBase.IsDeleted),
        nameof(AuditableDomainBase.CreatedAt),
        nameof(AuditableDomainBase.CreatedBy),
        nameof(AuditableDomainBase.UpdatedAt),
        nameof(AuditableDomainBase.UpdatedBy),
        nameof(AggregateRoot.RowVersion)
    ];

    public static List<AuditEntry> Capture(ChangeTracker tracker, AuditActorInfo actor)
    {
        tracker.DetectChanges();

        var entries = new List<AuditEntry>();
        foreach (var entry in tracker.Entries<DomainBase>().ToList())
        {
            var change = ChangeOf(entry);
            if (change is not null)
                entries.Add(AuditEntry.Record(change, actor, (entry.Entity as IAuditCompanyScope)?.AuditCompanyId));
        }

        return entries;
    }

    private static AuditChange? ChangeOf(EntityEntry<DomainBase> entry)
    {
        if (entry.State is not (EntityState.Added or EntityState.Modified or EntityState.Deleted)
            || entry.Metadata.ClrType.GetCustomAttribute<AuditIgnoreAttribute>() is not null)
            return null;

        var type = entry.Metadata.ClrType.Name;
        return entry.State switch
        {
            EntityState.Added => new AuditChange(type, entry.Entity.Id, AuditAction.Created, null, Json(Values(entry, current: true, changedOnly: false))),
            EntityState.Deleted => new AuditChange(type, entry.Entity.Id, AuditAction.Deleted, Json(Values(entry, current: false, changedOnly: false)), null),
            _ when IsSoftDeleted(entry) => new AuditChange(type, entry.Entity.Id, AuditAction.Deleted, Json(Values(entry, current: false, changedOnly: false)), null),
            _ => Update(entry, type)
        };
    }

    private static AuditChange? Update(EntityEntry<DomainBase> entry, string type)
    {
        var oldValues = Values(entry, current: false, changedOnly: true);
        return oldValues.Count == 0
            ? null
            : new AuditChange(type, entry.Entity.Id, AuditAction.Updated, Json(oldValues), Json(Values(entry, current: true, changedOnly: true)));
    }

    private static bool IsSoftDeleted(EntityEntry<DomainBase> entry) =>
        entry.Property(nameof(DomainBase.IsDeleted)).IsModified && entry.Entity.IsDeleted;

    private static Dictionary<string, object?> Values(EntityEntry<DomainBase> entry, bool current, bool changedOnly)
    {
        var values = new Dictionary<string, object?>();
        foreach (var property in entry.Properties)
        {
            if (IsExcluded(property) || (changedOnly && !property.IsModified))
                continue;

            values[SnakeCase.Convert(property.Metadata.Name)] = Plain(current ? property.CurrentValue : property.OriginalValue);
        }

        return values;
    }

    private static bool IsExcluded(PropertyEntry property) =>
        Bookkeeping.Contains(property.Metadata.Name)
        || property.Metadata.PropertyInfo?.GetCustomAttribute<AuditSecretAttribute>() is not null;

    /// <summary>An enum is written as its UPPER_SNAKE name, like the database does; everything else as it is.</summary>
    private static object? Plain(object? value) =>
        value is Enum e ? SnakeCase.ConvertUpper(e.ToString()) : value;

    private static string Json(Dictionary<string, object?> values) => JsonSerializer.Serialize(values);
}
