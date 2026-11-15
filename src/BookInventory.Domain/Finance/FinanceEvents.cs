using BookInventory.BuildingBlocks.Domain;

namespace BookInventory.Domain.Finance;

/// <summary>
/// Spec finance section 7: a document's transaction (or its reversal) was posted, naming the parties it touched. Raised by
/// <see cref="LedgerTransaction"/>; it carries ids and values, never entities.
/// </summary>
public sealed record LedgerPosted(string DocumentType, Guid DocumentId, PostingKind PostingKind, IReadOnlyList<LedgerParty> Parties) : DomainEventBase;
