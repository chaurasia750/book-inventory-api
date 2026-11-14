namespace BookInventory.Domain.Core;

/// <summary>
/// Marks a property whose value is never written to an audit entry (spec core A3): password hashes, tokens, temporary
/// passwords. The owning module puts it on its own property, next to the data it protects.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false)]
public sealed class AuditSecretAttribute : Attribute;
