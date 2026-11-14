using BookInventory.Domain.Core;

namespace BookInventory.Domain.Price;

/// <summary>
/// One named set of prices of one item (spec price R1-R9): MRP, sale price and purchase price from an effective date. A
/// company-owned aggregate root (platform P18): its company is stamped and filtered by the platform. It points at its item by id
/// only (R1). Every rule of the values (R2) is enforced here, and nothing outside changes its properties. A version is never
/// deleted (R3); its history is the audit trail of <c>core</c> (R15).
/// </summary>
public class PriceVersion : CompanyOwnedRoot
{
    public const int NameMinLength = 2;
    public const int NameMaxLength = 30;

    private PriceVersion()
    {
        VersionName = string.Empty;
        VersionNameNormalized = string.Empty;
    }

    public Guid ItemId { get; private set; }

    public string VersionName { get; private set; }

    /// <summary>R2: the name in the form two names are compared in (single spaces, lower case).</summary>
    public string VersionNameNormalized { get; private set; }

    public DateOnly EffectiveFrom { get; private set; }

    public decimal MrpAmount { get; private set; }

    public decimal SaleAmount { get; private set; }

    public decimal PurchaseAmount { get; private set; }

    /// <summary>R2: the form a version name is compared in; the store uses it to find a version with the same name.</summary>
    public static string NormalizeName(string? name) => PriceRules.NormalizeName(name);

    /// <summary>R2, R3: a new version of the item.</summary>
    public static PriceVersion Create(Guid itemId, PriceDetails details)
    {
        var version = new PriceVersion { ItemId = itemId };
        version.Apply(details);
        version.AddDomainEvent(new PriceVersionAdded(version.Id, itemId, version.VersionName));
        return version;
    }

    /// <summary>R3: changes the values in place; the old ones stay in the history. The item never changes.</summary>
    public void Edit(PriceDetails details)
    {
        Apply(details);
        AddDomainEvent(new PriceVersionChanged(Id, ItemId, VersionName));
    }

    private void Apply(PriceDetails details)
    {
        VersionName = PriceRules.RequireName(details.VersionName);
        VersionNameNormalized = PriceRules.NormalizeName(VersionName);
        EffectiveFrom = details.EffectiveFrom;
        MrpAmount = PriceRules.Amount(details.MrpAmount);
        SaleAmount = PriceRules.Amount(details.SaleAmount);
        PurchaseAmount = PriceRules.Amount(details.PurchaseAmount);
    }
}
