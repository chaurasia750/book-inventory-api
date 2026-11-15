namespace BookInventory.Application.Finance;

/// <summary>The party printed above the statement: a school (name, code, address, state, GSTIN) or a vendor (name, code, state, GSTIN).</summary>
public sealed record LedgerPdfParty(string Name, string Code, string? Address, string? State, string? Gstin);
