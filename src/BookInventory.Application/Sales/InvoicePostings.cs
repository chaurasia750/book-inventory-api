using BookInventory.Application.Company;
using BookInventory.Application.Core;
using BookInventory.Application.Finance;
using BookInventory.Application.Price;
using BookInventory.Application.School;
using BookInventory.Application.Stock;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.Domain.Finance;
using BookInventory.Domain.Sales;
using BookInventory.Domain.Stock;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Application.Sales;

/// <summary>R11, R12, D3: the invoice's stock out (stock R11) and ledger transaction (finance R5), and their reversal on a cancel.</summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class InvoicePostings(IStockPosting stock, ILedgerPosting ledger)
{
    public async Task<IReadOnlyList<Guid>> PostAsync(Invoice invoice, CancellationToken cancellationToken)
    {
        var result = await stock.PostAsync(
            new StockDocumentPosting(
                StockDocumentOf(invoice), invoice.InvoiceDate,
                [.. invoice.Lines.Select(l => new StockPostingLine(l.ItemId, StockType.Main, StockChange.Out(MovementKind.SchoolInvoice, l.Quantity)))]),
            cancellationToken);
        await ledger.PostAsync(InvoiceLedgerEntry.For(invoice), cancellationToken);
        return result.ItemsAtOrBelowZero;
    }

    public async Task ReverseAsync(Invoice invoice, DateOnly today, CancellationToken cancellationToken)
    {
        await ReverseStockAsync(invoice, today, cancellationToken);
        await ledger.ReverseAsync(new LedgerDocument(InvoiceLedgerEntry.DocumentType, invoice.Id, invoice.Number), today, cancellationToken);
    }

    /// <summary>An invoice of only non-stock items posted no stock entry; there is then nothing to reverse.</summary>
    private async Task ReverseStockAsync(Invoice invoice, DateOnly today, CancellationToken cancellationToken)
    {
        try
        {
            await stock.ReverseAsync(StockDocumentOf(invoice), today, cancellationToken);
        }
        catch (ConflictException ex) when (ex.Code == StockMessages.DocumentNotPosted)
        {
            // R2 of stock: lines of items that are not stock managed post nothing, so such an invoice has no stock to give back.
        }
    }

    private static StockDocument StockDocumentOf(Invoice invoice) => new(InvoiceLedgerEntry.DocumentType, invoice.Id, invoice.Number);
}
