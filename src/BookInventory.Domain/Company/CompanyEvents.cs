using BookInventory.BuildingBlocks.Domain;

namespace BookInventory.Domain.Company;

/// <summary>Domain events raised by <see cref="Company"/> (spec company section 7). They carry ids and values, never entities.</summary>
public abstract record CompanyEvent(Guid CompanyId) : DomainEventBase;

/// <summary>A company was created together with its Owner (R4).</summary>
public sealed record CompanyCreated(Guid CompanyId, string Name) : CompanyEvent(CompanyId);

/// <summary>A company was switched off (R7); its accounts' sessions are revoked by the use case, not by a handler.</summary>
public sealed record CompanyDeactivated(Guid CompanyId, string Reason) : CompanyEvent(CompanyId);

/// <summary>A deactivated company is usable again (R7).</summary>
public sealed record CompanyReactivated(Guid CompanyId) : CompanyEvent(CompanyId);
