namespace BookInventory.Application.Stock;

/// <summary>R17: the uploaded file as the use case sees it: its name, its size in bytes and its content.</summary>
public sealed record OpeningImportFile(string FileName, long Length, Stream Content);

/// <summary>R18: one wrong row of the file: its row number in the sheet, the column (<c>ItemCodeOrBarcode</c>, <c>StockType</c>, <c>Quantity</c>) and the message code.</summary>
public sealed record OpeningImportRowError(int RowNumber, string Column, string Code);

/// <summary>R18: how many openings were imported, or every wrong row (then nothing was imported).</summary>
public sealed record OpeningImportResult(int Imported, IReadOnlyList<OpeningImportRowError> Errors)
{
    public bool IsValid => Errors.Count == 0;
}
