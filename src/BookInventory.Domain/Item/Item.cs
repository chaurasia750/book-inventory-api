using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.Domain.Core;

namespace BookInventory.Domain.Item;

/// <summary>
/// One thing a company buys and sells (spec item R1-R15). A company-owned aggregate root (platform P18): its company is stamped
/// and filtered by the platform. It owns the master data (R2-R5, R7), the barcode list (R6) and the <c>ACTIVE</c>/<c>INACTIVE</c>
/// status (R8); every rule of those is enforced here, and nothing outside changes its properties. The code is generated once at
/// creation from the company code series (platform P23) and never changes (R10). It points at its vendor by id only (R5).
/// </summary>
public class Item : CompanyOwnedRoot
{
    public const int NameMinLength = 2;
    public const int NameMaxLength = 200;
    public const int CodeMaxLength = 20;
    public const int ClassMaxLength = 50;
    public const int SubjectMaxLength = 100;

    /// <summary>R10: the prefix of the item codes in the company code series (<c>ITM-0001</c>).</summary>
    public const string CodePrefix = "ITM";

    private readonly List<ItemBarcode> _barcodes = [];

    private Item()
    {
        Code = string.Empty;
        Name = string.Empty;
    }

    public string Code { get; private set; }

    public string Name { get; private set; }

    public ItemCategory Category { get; private set; }

    /// <summary>R5: the vendor of the same company, by id; none for an own-brand item.</summary>
    public Guid? VendorId { get; private set; }

    public string? ClassName { get; private set; }

    public string? Subject { get; private set; }

    public string? HsnCode { get; private set; }

    public decimal GstRatePercent { get; private set; }

    public bool IsStockManaged { get; private set; }

    public ItemStatus Status { get; private set; }

    /// <summary>The live barcodes in alphabetical order; soft-deleted rows stay in the backing field until the save and are not shown.</summary>
    public IReadOnlyList<ItemBarcode> Barcodes => _barcodes.Where(b => !b.IsDeleted).OrderBy(b => b.Barcode).ToList();

    public bool IsActive => Status == ItemStatus.Active;

    /// <summary>R6: the stored form of a barcode (upper case); callers compare barcodes in this form.</summary>
    public static string NormalizeBarcode(string? barcode) => ItemRules.Barcode(barcode);

    /// <summary>R2-R8, R10: a new item, active, with the generated code; stock managed follows the category unless given.</summary>
    public static Item Create(string code, ItemDetails details)
    {
        var item = new Item { Code = code, Status = ItemStatus.Active };
        item.Apply(details, details.IsStockManaged ?? ItemCategoryRules.IsStockManagedByDefault(details.Category));
        item.AddDomainEvent(new ItemCreated(item.Id, item.Code, item.Name));
        return item;
    }

    /// <summary>R2-R5, R7: changes the master data; the code, the status and the barcodes never change here.</summary>
    public void Edit(ItemDetails details, bool isStockManaged) => Apply(details, isStockManaged);

    /// <summary>
    /// R6: the whole barcode list. Barcodes are distinct inside the item; a barcode that stays keeps its row, a new one is added,
    /// one that is gone is soft-deleted (platform P4). That a barcode is free in the company is checked by the use case.
    /// </summary>
    public void ReplaceBarcodes(IEnumerable<string> barcodes)
    {
        var cleaned = barcodes.Select(ItemRules.Barcode).ToList();
        if (cleaned.Distinct().Count() != cleaned.Count)
            throw new ValidationFailureException(ItemMessages.DuplicateBarcode);

        foreach (var existing in _barcodes.Where(b => !b.IsDeleted && !cleaned.Contains(b.Barcode)))
            existing.MarkDeleted();

        foreach (var barcode in cleaned.Where(c => !_barcodes.Any(b => !b.IsDeleted && b.Barcode == c)))
            _barcodes.Add(ItemBarcode.Of(Id, barcode));
    }

    /// <summary>R8: switches the item off; its data and history stay.</summary>
    public void Deactivate()
    {
        if (Status == ItemStatus.Inactive)
            throw new ConflictException(ItemMessages.AlreadyInactive);

        Status = ItemStatus.Inactive;
        AddDomainEvent(new ItemDeactivated(Id));
    }

    /// <summary>R8: switches the item on again.</summary>
    public void Reactivate()
    {
        if (Status == ItemStatus.Active)
            throw new ConflictException(ItemMessages.AlreadyActive);

        Status = ItemStatus.Active;
        AddDomainEvent(new ItemReactivated(Id));
    }

    private void Apply(ItemDetails details, bool isStockManaged)
    {
        Name = ItemRules.RequireName(details.Name);
        Category = details.Category;
        VendorId = details.VendorId;
        ClassName = ItemRules.Optional(details.ClassName, ClassMaxLength);
        Subject = ItemRules.Optional(details.Subject, SubjectMaxLength);
        HsnCode = ItemRules.Hsn(details.HsnCode);
        GstRatePercent = ItemRules.GstRate(details.GstRatePercent);
        IsStockManaged = isStockManaged;
    }
}
