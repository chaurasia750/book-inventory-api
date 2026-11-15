using BookInventory.Domain.Core;

namespace BookInventory.Domain.Planning;

/// <summary>
/// Spec planning R2: one book of a <see cref="ClassBookList"/>: the item (kept by id with its code and name for reading) and how
/// many copies each student needs. A company-owned child (platform P18), changed only by its list.
/// </summary>
public class ClassBookListLine : CompanyOwnedChild
{
    private ClassBookListLine()
    {
        ItemCode = string.Empty;
        ItemName = string.Empty;
    }

    public Guid ClassBookListId { get; private set; }

    public int SortOrder { get; private set; }

    public Guid ItemId { get; private set; }

    public string ItemCode { get; private set; }

    public string ItemName { get; private set; }

    public int QuantityPerStudent { get; private set; }

    internal static ClassBookListLine Of(Guid listId, int sortOrder, BookListLineInput input) =>
        new()
        {
            ClassBookListId = listId,
            SortOrder = sortOrder,
            ItemId = input.ItemId,
            ItemCode = input.ItemCode,
            ItemName = input.ItemName,
            QuantityPerStudent = input.QuantityPerStudent
        };
}
