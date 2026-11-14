namespace BookInventory.Domain.Core;

/// <summary>
/// Implemented by an entity that belongs to a company (spec core A5): the company itself, an Owner or Staff account, and
/// later every company-owned business row. The audit entry of a change takes its company from here; an entity without it
/// is a platform row and its entries have no company.
/// </summary>
public interface IAuditCompanyScope
{
    /// <summary>The company this row belongs to, or null (a Super Admin account).</summary>
    Guid? AuditCompanyId { get; }
}
