using ItemRoot = BookInventory.Domain.Item.Item;

namespace BookInventory.Application.Item;

/// <summary>R22: the state of one import run: the wrong rows so far, the new items, the number of updates and what the file used already.</summary>
public sealed class ItemImportRun
{
    public List<ItemImportRowError> Errors { get; } = [];

    public List<ItemRoot> Created { get; } = [];

    public int Updated { get; set; }

    public HashSet<Guid> SeenItems { get; } = [];

    public HashSet<string> SeenBarcodes { get; } = [];
}
