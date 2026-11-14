namespace BookInventory.Domain.Price;

/// <summary>What the default-version rule needs to know about a version.</summary>
public sealed record VersionStamp(Guid Id, DateOnly EffectiveFrom, DateTime CreatedAt);

/// <summary>
/// R4 (BR-PRC-02): the default version of an item is the one with the latest effective date that is not after the business
/// date; when two share that date the one created later wins; none when every version is still in the future. Written once, so
/// the single-item read and the price list agree.
/// </summary>
public static class DefaultPrice
{
    public static Guid? IdOf(IEnumerable<VersionStamp> versions, DateOnly today) =>
        versions
            .Where(v => v.EffectiveFrom <= today)
            .OrderByDescending(v => v.EffectiveFrom)
            .ThenByDescending(v => v.CreatedAt)
            .Select(v => (Guid?)v.Id)
            .FirstOrDefault();
}
