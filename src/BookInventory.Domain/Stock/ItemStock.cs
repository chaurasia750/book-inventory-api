using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.Domain.Core;

namespace BookInventory.Domain.Stock;

/// <summary>
/// Spec stock R3-R9, R12, R15, D1: the stock of one item in one company: the running Main, Damaged and Sample quantities and the
/// entries that make them. Every movement goes through <see cref="Post"/>-style behaviours here, which validate it, append one
/// <see cref="StockEntry"/> and change the total in the same step, so a quantity always equals the sum of its entries (R6). Loading
/// it does not load the history: new entries are appended without reading the old ones. A quantity may go below zero (R9). The
/// totals are derived, so the audit trail skips them (R13).
/// </summary>
[AuditIgnore]
public class ItemStock : CompanyOwnedRoot
{
    /// <summary>R15: the largest quantity of one movement or one counted quantity; anything larger is a typing mistake.</summary>
    public const int MaxQuantity = 10_000_000;

    public const int ReasonMinLength = 5;
    public const int ReasonMaxLength = 500;
    public const int NoteMaxLength = 500;

    private readonly List<StockEntry> _entries = [];

    private ItemStock()
    {
    }

    public Guid ItemId { get; private set; }

    public int MainQuantity { get; private set; }

    public int DamagedQuantity { get; private set; }

    public int SampleQuantity { get; private set; }

    /// <summary>The entries appended in this unit of work; the stored history is read through the query side, not through this list.</summary>
    public IReadOnlyList<StockEntry> Entries => _entries;

    /// <summary>D1: the stock card of an item, created on its first movement with every quantity at zero.</summary>
    public static ItemStock StartFor(Guid itemId)
    {
        if (itemId == Guid.Empty)
            throw new ValidationFailureException(StockMessages.ItemNotFound);

        return new ItemStock { ItemId = itemId };
    }

    public int QuantityOf(StockType stockType) =>
        stockType switch
        {
            StockType.Main => MainQuantity,
            StockType.Damaged => DamagedQuantity,
            StockType.Sample => SampleQuantity,
            _ => throw new ValidationFailureException(StockMessages.InvalidMovement)
        };

    /// <summary>R7: the opening quantity of one stock type, dated the day counting starts. "Only once" is checked by the caller and guarded by a unique index.</summary>
    public StockEntry EnterOpening(StockType stockType, int quantity, DateOnly openingDate, string? note = null)
    {
        RequireQuantity(quantity);
        return Append(new StockMovement
        {
            StockType = stockType,
            Change = StockChange.In(MovementKind.Opening, quantity),
            MovementDate = openingDate,
            Note = note
        });
    }

    /// <summary>R8, R12: the Owner's physical count; the difference is posted as one adjustment dated <paramref name="today"/>. An equal count is refused.</summary>
    public StockEntry Adjust(StockType stockType, int countedQuantity, string reason, DateOnly today)
    {
        if (countedQuantity is < 0 or > MaxQuantity)
            throw new ValidationFailureException(StockMessages.InvalidQuantity);
        var trimmed = reason?.Trim() ?? string.Empty;
        if (trimmed.Length is < ReasonMinLength or > ReasonMaxLength)
            throw new ValidationFailureException(StockMessages.InvalidReason);

        var difference = countedQuantity - QuantityOf(stockType);
        if (difference == 0)
            throw new ValidationFailureException(StockMessages.NothingToAdjust);

        var change = difference > 0
            ? StockChange.In(MovementKind.Adjustment, difference)
            : StockChange.Out(MovementKind.Adjustment, -difference);
        return Append(new StockMovement { StockType = stockType, Change = change, MovementDate = today, Note = trimmed });
    }

    /// <summary>R5, R9, R11: a document's movement (purchase, invoice, return, sample...). Never refused for low stock.</summary>
    public StockEntry Post(StockMovement movement)
    {
        if (movement.Change.Kind is MovementKind.Opening or MovementKind.Adjustment or MovementKind.Reversal)
            throw new ValidationFailureException(StockMessages.InvalidMovement);
        if (movement.Document is null)
            throw new ValidationFailureException(StockMessages.InvalidDocument);

        return Append(movement);
    }

    /// <summary>R11: undoes one entry a cancelled document posted, with a <see cref="MovementKind.Reversal"/> entry of the opposite direction.</summary>
    public StockEntry Reverse(StockType stockType, StockChange original, StockDocument document, DateOnly reversalDate)
    {
        if (original.Kind is MovementKind.Reversal)
            throw new ValidationFailureException(StockMessages.InvalidMovement);

        return Append(new StockMovement
        {
            StockType = stockType,
            Change = original.Reversed(),
            MovementDate = reversalDate,
            Document = document
        });
    }

    private StockEntry Append(StockMovement movement)
    {
        Validate(movement);

        var entry = StockEntry.Of(this, movement);
        _entries.Add(entry);
        var quantity = checked(QuantityOf(movement.StockType) + movement.Change.Net);
        SetQuantity(movement.StockType, quantity);

        AddDomainEvent(new StockMoved(ItemId, movement.StockType, movement.Change, quantity));
        return entry;
    }

    private static void Validate(StockMovement movement)
    {
        var change = movement.Change;
        if (change.QuantityIn < 0 || change.QuantityOut < 0 || (change.QuantityIn > 0) == (change.QuantityOut > 0))
            throw new ValidationFailureException(StockMessages.InvalidQuantity);
        RequireQuantity(Math.Max(change.QuantityIn, change.QuantityOut));
        if (!MovementKindRules.Allows(change.Kind, movement.StockType, change.IsIn))
            throw new ValidationFailureException(StockMessages.InvalidMovement);
        if (movement.MovementDate == default)
            throw new ValidationFailureException(StockMessages.InvalidDate);
        if (movement.Note?.Length > NoteMaxLength)
            throw new ValidationFailureException(StockMessages.InvalidReason);
        if (movement.Document is { } document && !IsValid(document))
            throw new ValidationFailureException(StockMessages.InvalidDocument);
    }

    private static bool IsValid(StockDocument document) =>
        document.DocumentId != Guid.Empty
        && !string.IsNullOrWhiteSpace(document.DocumentType) && document.DocumentType.Length <= StockDocument.TypeMaxLength
        && !string.IsNullOrWhiteSpace(document.DocumentNumber) && document.DocumentNumber.Length <= StockDocument.NumberMaxLength;

    private static void RequireQuantity(int quantity)
    {
        if (quantity is <= 0 or > MaxQuantity)
            throw new ValidationFailureException(StockMessages.InvalidQuantity);
    }

    private void SetQuantity(StockType stockType, int quantity)
    {
        switch (stockType)
        {
            case StockType.Main: MainQuantity = quantity; break;
            case StockType.Damaged: DamagedQuantity = quantity; break;
            case StockType.Sample: SampleQuantity = quantity; break;
            default: throw new ValidationFailureException(StockMessages.InvalidMovement);
        }
    }
}
