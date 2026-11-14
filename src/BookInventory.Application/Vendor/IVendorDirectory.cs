namespace BookInventory.Application.Vendor;

/// <summary>R15: what another module may know about a vendor: its id, code, name and whether it can be chosen for a new document.</summary>
public sealed record VendorReference(Guid Id, string Code, string Name, bool IsActive);

/// <summary>
/// R15: the capability other modules (the item master first) use to read vendors, never by joining the vendor tables. It
/// answers only for the company of the session (platform P19), returns plain values and never the aggregate, and changes nothing.
/// </summary>
public interface IVendorDirectory
{
    /// <summary>The vendor's id, name and active flag, or <c>null</c> for an unknown id or another company's.</summary>
    Task<VendorReference?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>R15 (for the ledger, finance R11): the references of the given vendors, by id; an id that is not found is left out.</summary>
    Task<IReadOnlyDictionary<Guid, VendorReference>> ReferencesAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default);

    /// <summary>The names of the given vendors of the company, by id; an id that is not found is left out.</summary>
    Task<IReadOnlyDictionary<Guid, string>> NamesAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default);

    /// <summary>The ids of the company's vendors whose name or code contains the text, at most <see cref="MaxMatches"/>.</summary>
    Task<IReadOnlyList<Guid>> IdsMatchingAsync(string text, CancellationToken cancellationToken = default);

    /// <summary>The cap of <see cref="IdsMatchingAsync"/>.</summary>
    const int MaxMatches = 200;
}
