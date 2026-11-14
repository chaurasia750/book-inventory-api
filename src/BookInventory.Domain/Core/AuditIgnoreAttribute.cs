namespace BookInventory.Domain.Core;

/// <summary>
/// Marks an entity that is never audited (spec core AQ1): the insert-only history and log tables, the session and token
/// tables and the audit entries themselves. The owning module puts it on its own class.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class AuditIgnoreAttribute : Attribute;
