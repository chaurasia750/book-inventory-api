using BookInventory.Application.Stock;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.Domain.Sales;
using BookInventory.Domain.Stock;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Application.Sales;

/// <summary>
/// R46, R47, D13: the counter bill's stock out (stock R11: Main, <c>COUNTER_SALE</c>) and its reversal on a cancel. A counter bill
/// posts nothing to any ledger (FR-CNT-10, BR-LED-03), so the stock is all there is to post or give back.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class CounterBillPostings(IStockPosting stock)
{
    public async Task<IReadOnlyList<Guid>> PostAsync(CounterBill bill, CancellationToken cancellationToken)
    {
        var result = await stock.PostAsync(
            new StockDocumentPosting(
                DocumentOf(bill), bill.BillDate,
                [.. bill.Lines.Select(l => new StockPostingLine(l.ItemId, StockType.Main, StockChange.Out(MovementKind.CounterSale, l.Quantity)))]),
            cancellationToken);
        return result.ItemsAtOrBelowZero;
    }

    /// <summary>A bill of only non-stock items posted no stock entry; there is then nothing to give back.</summary>
    public async Task ReverseAsync(CounterBill bill, DateOnly today, CancellationToken cancellationToken)
    {
        try
        {
            await stock.ReverseAsync(DocumentOf(bill), today, cancellationToken);
        }
        catch (ConflictException ex) when (ex.Code == StockMessages.DocumentNotPosted)
        {
            // Stock R2: lines of items that are not stock managed post nothing, so such a bill has no stock to give back.
        }
    }

    private static StockDocument DocumentOf(CounterBill bill) => new(CounterBill.DocumentType, bill.Id, bill.Number);
}
