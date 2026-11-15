namespace BookInventory.Application.Item;

/// <summary>R18: the uploaded file as the use case sees it: its name, its size in bytes and its content.</summary>
public sealed record ItemImportFile(string FileName, long Length, Stream Content);

/// <summary>R22: one wrong row: its row number in the sheet, the column (<c>ItemCode</c>, <c>Barcode</c>, <c>Name</c> ...) and the message code.</summary>
public sealed record ItemImportRowError(int RowNumber, string Column, string Code);

/// <summary>R22: how many items were created and updated, or every wrong row (then nothing was changed).</summary>
public sealed record ItemImportResult(int Created, int Updated, IReadOnlyList<ItemImportRowError> Errors)
{
    public bool IsValid => Errors.Count == 0;
}
