using BookInventory.Application.Finance;
using BookInventory.Application.Stock;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.Domain.Finance;
using BookInventory.Domain.Purchase;
using BookInventory.Domain.Stock;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Application.Purchase;

/// <summary>R6, R7, R8: the vendor return's stock out (stock R11: `VENDOR_RETURN` from Main or Damaged) and ledger transaction (finance R5), and their reversal on a cancel.</summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class VendorReturnPostings(IStockPosting stock, ILedgerPosting ledger)
{
    public async Task<IReadOnlyList<Guid>> PostAsync(VendorReturn vendorReturn, CancellationToken cancellationToken)
    {
        var result = await stock.PostAsync(new StockDocumentPosting(StockDocumentOf(vendorReturn), vendorReturn.ReturnDate, [.. vendorReturn.Lines.Select(StockLine)]), cancellationToken);
        await ledger.PostAsync(VendorReturnLedgerEntry.For(vendorReturn), cancellationToken);
        return result.ItemsAtOrBelowZero;
    }

    public async Task ReverseAsync(VendorReturn vendorReturn, DateOnly today, CancellationToken cancellationToken)
    {
        await ReverseStockAsync(vendorReturn, today, cancellationToken);
        await ledger.ReverseAsync(new LedgerDocument(VendorReturn.DocumentType, vendorReturn.Id, vendorReturn.Number), today, cancellationToken);
    }

    private static StockPostingLine StockLine(VendorReturnLine line) =>
        new(line.ItemId, line.StockSource == ReturnStockSource.Main ? StockType.Main : StockType.Damaged, StockChange.Out(MovementKind.VendorReturn, line.Quantity));

    /// <summary>A vendor return of only non-stock items posted no stock entry; there is then nothing to give back (stock R2).</summary>
    private async Task ReverseStockAsync(VendorReturn vendorReturn, DateOnly today, CancellationToken cancellationToken)
    {
        try
        {
            await stock.ReverseAsync(StockDocumentOf(vendorReturn), today, cancellationToken);
        }
        catch (ConflictException ex) when (ex.Code == StockMessages.DocumentNotPosted)
        {
            // Stock R2: lines of items that are not stock managed post nothing, so such a vendor return has no stock to give back.
        }
    }

    private static StockDocument StockDocumentOf(VendorReturn vendorReturn) => new(VendorReturn.DocumentType, vendorReturn.Id, vendorReturn.Number);
}
