namespace BookInventory.Domain.Planning;

/// <summary>R2: one book of a class book list as the use case hands it to the list: the item (with its code and name for reading) and the quantity per student.</summary>
public sealed record BookListLineInput(Guid ItemId, string ItemCode, string ItemName, int QuantityPerStudent);
