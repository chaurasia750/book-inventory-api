using BookInventory.Application.Core;
using BookInventory.Application.Vendor;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.Domain.Item;
using Microsoft.Extensions.DependencyInjection;
using ItemRoot = BookInventory.Domain.Item.Item;

namespace BookInventory.Application.Item;

/// <summary>
/// R18-R22: the Excel import and bulk update of items. The file is checked as a whole (type, size, row count), then every row
/// (format, item by code or barcode, vendor by name, not twice in the file) and applied through the same item behaviours as a
/// typed one (Create, Edit, ReplaceBarcodes). Any wrong row is reported with all the others and the request is answered 400,
/// which rolls the transaction back, so nothing is changed; otherwise the new items are added.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class ItemImportService(IItemStore store, ICompanyCodes codes, IVendorDirectory vendors, ISpreadsheetReader reader)
{
    public const long MaxFileBytes = 5 * 1024 * 1024;
    public const int MaxRows = 5000;

    public async Task<ItemImportResult> ImportAsync(ItemImportFile file, CancellationToken cancellationToken)
    {
        var rows = await ReadAsync(file, cancellationToken);
        var run = new ItemImportRun();
        var parsed = new List<ItemImportRow>();
        foreach (var (row, error) in rows.Select(ItemImportRow.Parse))
        {
            if (error is not null) run.Errors.Add(error);
            else parsed.Add(row!);
        }

        var lookup = await LookupAsync(parsed, cancellationToken);
        foreach (var row in parsed)
            await ApplyAsync(row, lookup, run, cancellationToken);

        if (run.Errors.Count > 0)
            return new ItemImportResult(0, 0, [.. run.Errors.OrderBy(e => e.RowNumber)]);

        foreach (var item in run.Created)
            await store.AddAsync(item, cancellationToken);
        return new ItemImportResult(run.Created.Count, run.Updated, []);
    }

    /// <summary>R18: the file limits (type, size, readable, row count, not empty); a wrong file is refused as a whole.</summary>
    private async Task<IReadOnlyList<SpreadsheetRow>> ReadAsync(ItemImportFile file, CancellationToken cancellationToken)
    {
        if (!string.Equals(Path.GetExtension(file.FileName), ".xlsx", StringComparison.OrdinalIgnoreCase))
            throw new ValidationFailureException(ItemMessages.ImportInvalidFile);
        if (file.Length > MaxFileBytes)
            throw new ValidationFailureException(ItemMessages.ImportTooLarge);

        var content = await reader.ReadFirstSheetAsync(file.Content, MaxRows, cancellationToken);
        if (!content.IsReadable)
            throw new ValidationFailureException(ItemMessages.ImportInvalidFile);
        if (content.HasTooManyRows)
            throw new ValidationFailureException(ItemMessages.ImportTooManyRows);
        if (content.Rows.Count == 0)
            throw new ValidationFailureException(ItemMessages.ImportEmpty);

        return content.Rows;
    }

    private async Task<ItemImportLookup> LookupAsync(IReadOnlyList<ItemImportRow> rows, CancellationToken cancellationToken)
    {
        var keys = rows.SelectMany(r => new[] { r.ItemCode, r.Barcode }).OfType<string>().Distinct().ToList();
        var matches = await store.MatchKeysAsync(keys, cancellationToken);
        var byCode = matches.Where(m => m.IsCode).ToDictionary(m => m.Key, m => m.ItemId);
        var byBarcode = matches.Where(m => !m.IsCode).GroupBy(m => m.Key).ToDictionary(g => g.Key, g => g.First().ItemId);

        var items = await store.FindManyForManagementAsync([.. byCode.Values.Concat(byBarcode.Values).Distinct()], cancellationToken);
        var names = rows.Select(r => r.VendorName.Value).OfType<string>().ToList();
        return new ItemImportLookup(byCode, byBarcode, items.ToDictionary(i => i.Id), await vendors.FindByNamesAsync(names, cancellationToken));
    }

    private async Task ApplyAsync(ItemImportRow row, ItemImportLookup lookup, ItemImportRun run, CancellationToken cancellationToken)
    {
        var error = ResolveTarget(row, lookup, run, out var item);
        if (error is null && TryVendor(row, item, lookup, out var vendorId, out error))
            error = await TryApplyAsync(row, item, vendorId, run, cancellationToken);

        if (error is not null)
            run.Errors.Add(error);
    }

    /// <summary>R20: the aggregate checks the values it owns; its refusal becomes the row's error.</summary>
    private async Task<ItemImportRowError?> TryApplyAsync(
        ItemImportRow row, ItemRoot? item, Guid? vendorId, ItemImportRun run, CancellationToken cancellationToken)
    {
        try
        {
            if (item is null) await CreateAsync(row, vendorId, run, cancellationToken);
            else Update(row, item, vendorId, run);
            return null;
        }
        catch (ValidationFailureException failure)
        {
            return row.ErrorOf(ColumnOf(failure.Code), failure.Code);
        }
    }

    /// <summary>R19: the item the row changes (null for a new one); the same item or barcode twice, or a barcode of another item, is wrong.</summary>
    private static ItemImportRowError? ResolveTarget(ItemImportRow row, ItemImportLookup lookup, ItemImportRun run, out ItemRoot? item)
    {
        item = null;
        Guid? id = null;
        if (row.ItemCode is not null)
        {
            if (!lookup.ByCode.TryGetValue(row.ItemCode, out var byCode))
                return row.ErrorOf(ItemImportRow.ItemCodeColumn, ItemMessages.ImportItemUnknown);
            id = byCode;
        }
        else if (row.Barcode is not null && lookup.ByBarcode.TryGetValue(row.Barcode, out var byBarcode))
        {
            id = byBarcode;
        }

        item = id is { } known ? lookup.Items[known] : null;
        if (id is { } seen && !run.SeenItems.Add(seen))
            return row.ErrorOf(row.ItemCode is null ? ItemImportRow.BarcodeColumn : ItemImportRow.ItemCodeColumn, ItemMessages.ImportDuplicateRow);

        return BarcodeError(row, lookup, run, id);
    }

    private static ItemImportRowError? BarcodeError(ItemImportRow row, ItemImportLookup lookup, ItemImportRun run, Guid? targetId)
    {
        if (row.Barcode is null)
            return null;
        if (!run.SeenBarcodes.Add(row.Barcode))
            return row.ErrorOf(ItemImportRow.BarcodeColumn, ItemMessages.ImportDuplicateRow);

        return lookup.ByBarcode.TryGetValue(row.Barcode, out var owner) && owner != targetId
            ? row.ErrorOf(ItemImportRow.BarcodeColumn, ItemMessages.BarcodeTaken)
            : null;
    }

    /// <summary>R5, R20, R21: the vendor named in the row (a newly chosen one must be active), the current one when the cell is empty, none when it clears.</summary>
    private static bool TryVendor(ItemImportRow row, ItemRoot? item, ItemImportLookup lookup, out Guid? vendorId, out ItemImportRowError? error)
    {
        error = null;
        vendorId = item?.VendorId;
        if (row.VendorName.IsClear)
        {
            vendorId = null;
        }
        else if (row.VendorName.Value is { } name)
        {
            var found = lookup.Vendors.GetValueOrDefault(Domain.Vendor.Vendor.NormalizeName(name));
            if (found is null || (!found.IsActive && found.Id != item?.VendorId))
                error = row.ErrorOf(ItemImportRow.VendorColumn, ItemMessages.ImportVendorUnknown);
            vendorId = found?.Id;
        }

        return error is null;
    }

    private async Task CreateAsync(ItemImportRow row, Guid? vendorId, ItemImportRun run, CancellationToken cancellationToken)
    {
        if (row.Name is null)
            throw new ValidationFailureException(ItemMessages.ImportNameRequired);
        if (row.Category is null)
            throw new ValidationFailureException(ItemMessages.ImportInvalidCategory);

        var details = new ItemDetails(
            row.Name, row.Category.Value, vendorId, row.ClassName.ApplyTo(null), row.Subject.ApplyTo(null),
            row.HsnCode.ApplyTo(null), row.GstRatePercent, row.IsStockManaged);
        var item = ItemRoot.Create(await codes.NextAsync(ItemRoot.CodePrefix, cancellationToken), details);
        if (row.Barcode is not null)
            item.ReplaceBarcodes([row.Barcode]);
        run.Created.Add(item);
    }

    /// <summary>R21: a blank cell keeps the current value, a dash clears an optional one, a barcode is added to the item's own.</summary>
    private static void Update(ItemImportRow row, ItemRoot item, Guid? vendorId, ItemImportRun run)
    {
        var isStockManaged = row.IsStockManaged ?? item.IsStockManaged;
        var details = new ItemDetails(
            row.Name ?? item.Name, row.Category ?? item.Category, vendorId, row.ClassName.ApplyTo(item.ClassName), row.Subject.ApplyTo(item.Subject),
            row.HsnCode.ApplyTo(item.HsnCode), row.GstRatePercent ?? item.GstRatePercent, isStockManaged);
        item.Edit(details, isStockManaged);
        if (row.Barcode is not null)
            item.ReplaceBarcodes(item.Barcodes.Select(b => b.Barcode).Append(row.Barcode).Distinct());
        run.Updated++;
    }

    private static string ColumnOf(string code) => code switch
    {
        ItemMessages.InvalidHsn => ItemImportRow.HsnColumn,
        ItemMessages.InvalidGstRate => ItemImportRow.GstColumn,
        ItemMessages.InvalidBarcode => ItemImportRow.BarcodeColumn,
        ItemMessages.ImportInvalidCategory => ItemImportRow.CategoryColumn,
        _ => ItemImportRow.NameColumn
    };
}
