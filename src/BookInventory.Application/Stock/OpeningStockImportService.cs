using BookInventory.Application.Core;
using BookInventory.Application.Item;
using BookInventory.Application.Price;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.Domain.Stock;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Application.Stock;

/// <summary>
/// R17-R19: the Excel opening stock. The file is checked as a whole (type, size, row count), then every row (format, item by code
/// or barcode, stock managed, no opening yet, not twice in the file) before anything is written. Any wrong row means nothing is
/// imported and every wrong row is reported; otherwise every row becomes an opening through the same <see cref="ItemStock"/>
/// behaviour as a typed one (R19), in the request transaction.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class OpeningStockImportService(
    IStockStore store, StockCards cards, IItemDirectory items, ISpreadsheetReader reader, PriceCompanyGuard companyGuard)
{
    public const long MaxFileBytes = 5 * 1024 * 1024;
    public const int MaxRows = 5000;
    private const int NoteMaxFileName = 200;

    public async Task<OpeningImportResult> ImportAsync(OpeningImportFile file, DateOnly openingDate, CancellationToken cancellationToken)
    {
        companyGuard.Require();
        var rows = await ReadAsync(file, cancellationToken);

        var errors = new List<OpeningImportRowError>();
        var parsed = new List<OpeningImportRow>();
        foreach (var (row, error) in rows.Select(OpeningImportRow.Parse))
        {
            if (error is not null) errors.Add(error);
            else parsed.Add(row!);
        }

        var found = await items.FindByCodeOrBarcodeAsync([.. parsed.Select(r => r.Key).Distinct()], cancellationToken);
        var openings = (await store.OpeningsOfAsync([.. found.Values.Select(i => i.Id).Distinct()], cancellationToken)).ToHashSet();
        var accepted = Accept(parsed, found, openings, errors);
        if (errors.Count > 0)
            return new OpeningImportResult(0, [.. errors.OrderBy(e => e.RowNumber)]);

        var stocks = await cards.ForItemsAsync([.. accepted.Select(a => a.ItemId).Distinct()], cancellationToken);
        var note = NoteOf(file.FileName);
        foreach (var (row, itemId) in accepted)
            stocks[itemId].EnterOpening(row.StockType, row.Quantity, openingDate, note);

        return new OpeningImportResult(accepted.Count, []);
    }

    /// <summary>R19: the note of an imported opening names the file (its name only, cut to fit the note).</summary>
    private static string NoteOf(string fileName)
    {
        var name = Path.GetFileName(fileName);
        return name.Length > NoteMaxFileName ? name[..NoteMaxFileName] : name;
    }

    /// <summary>R17: the file limits (type, size, readable, row count, not empty); a wrong file is refused as a whole.</summary>
    private async Task<IReadOnlyList<SpreadsheetRow>> ReadAsync(OpeningImportFile file, CancellationToken cancellationToken)
    {
        if (!string.Equals(Path.GetExtension(file.FileName), ".xlsx", StringComparison.OrdinalIgnoreCase))
            throw new ValidationFailureException(StockMessages.ImportInvalidFile);
        if (file.Length > MaxFileBytes)
            throw new ValidationFailureException(StockMessages.ImportTooLarge);

        var content = await reader.ReadFirstSheetAsync(file.Content, MaxRows, cancellationToken);
        if (!content.IsReadable)
            throw new ValidationFailureException(StockMessages.ImportInvalidFile);
        if (content.HasTooManyRows)
            throw new ValidationFailureException(StockMessages.ImportTooManyRows);
        if (content.Rows.Count == 0)
            throw new ValidationFailureException(StockMessages.ImportEmpty);

        return content.Rows;
    }

    /// <summary>R2, R7, R18: the rows whose item is known and stock managed, with no opening yet and not earlier in the file.</summary>
    private static List<(OpeningImportRow Row, Guid ItemId)> Accept(
        IReadOnlyList<OpeningImportRow> rows, IReadOnlyDictionary<string, ItemReference> found, IReadOnlySet<StockKey> openings, List<OpeningImportRowError> errors)
    {
        var accepted = new List<(OpeningImportRow, Guid)>();
        var seen = new HashSet<StockKey>();
        foreach (var row in rows)
        {
            var code = CheckRow(row, found.GetValueOrDefault(row.Key), openings, seen);
            if (code is null)
                accepted.Add((row, found[row.Key].Id));
            else
                errors.Add(row.ErrorOf(code == StockMessages.ImportDuplicateRow || code == StockMessages.OpeningExists ? OpeningImportRow.StockTypeColumn : OpeningImportRow.KeyColumn, code));
        }

        return accepted;
    }

    private static string? CheckRow(OpeningImportRow row, ItemReference? item, IReadOnlySet<StockKey> openings, HashSet<StockKey> seen)
    {
        if (item is null)
            return StockMessages.ImportItemUnknown;
        if (!item.IsStockManaged)
            return StockMessages.NotStockManaged;

        var key = new StockKey(item.Id, row.StockType);
        if (openings.Contains(key))
            return StockMessages.OpeningExists;

        return seen.Add(key) ? null : StockMessages.ImportDuplicateRow;
    }
}
