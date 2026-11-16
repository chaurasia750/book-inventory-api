namespace BookInventory.Application.Price;

/// <summary>R19: an item's purchase price in its default version on a date, with the version's id and name; never the sale price or the MRP.</summary>
public sealed record PurchasePriceReference(Guid ItemId, Guid VersionId, string VersionName, decimal PurchaseAmount);

/// <summary>
/// R19: the capability the purchase module uses to start the rate of a vendor-return line, never by joining the price tables. It
/// answers only for the session's company, returns plain values, never the sale price or the MRP (<see cref="IPriceDirectory"/>
/// is the one for those), and changes nothing.
/// </summary>
public interface IPurchasePriceDirectory
{
    /// <summary>The cap of <see cref="PurchasePricesAsync"/>.</summary>
    const int MaxItems = 500;

    /// <summary>
    /// The purchase price of each item in its default version on <paramref name="onDate"/> (R4); an item without a version on that date
    /// is left out; more than <see cref="MaxItems"/> ids is <c>400 price.too_many_items</c>.
    /// </summary>
    Task<IReadOnlyDictionary<Guid, PurchasePriceReference>> PurchasePricesAsync(IReadOnlyCollection<Guid> itemIds, DateOnly onDate, CancellationToken cancellationToken = default);
}
