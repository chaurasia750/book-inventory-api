using BookInventory.Application.Finance;
using BookInventory.Application.Stock;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.Domain.Finance;
using BookInventory.Domain.Sales;
using BookInventory.Domain.Stock;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Application.Sales;

/// <summary>R57, R58: the credit note's stock in (good to Main, damaged to Damaged; stock R11) and ledger transaction (finance R5), and their reversal on a cancel.</summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class CreditNotePostings(IStockPosting stock, ILedgerPosting ledger)
{
    public async Task PostAsync(CreditNote note, CancellationToken cancellationToken)
    {
        await stock.PostAsync(new StockDocumentPosting(StockDocumentOf(note), note.NoteDate, [.. note.Lines.Select(StockLine)]), cancellationToken);
        await ledger.PostAsync(CreditNoteLedgerEntry.For(note), cancellationToken);
    }

    public async Task ReverseAsync(CreditNote note, DateOnly today, CancellationToken cancellationToken)
    {
        await ReverseStockAsync(note, today, cancellationToken);
        await ledger.ReverseAsync(new LedgerDocument(CreditNote.DocumentType, note.Id, note.Number), today, cancellationToken);
    }

    private static StockPostingLine StockLine(CreditNoteLine line) =>
        line.Condition == ReturnCondition.Good
            ? new StockPostingLine(line.ItemId, StockType.Main, StockChange.In(MovementKind.SchoolReturnGood, line.Quantity))
            : new StockPostingLine(line.ItemId, StockType.Damaged, StockChange.In(MovementKind.SchoolReturnDamaged, line.Quantity));

    /// <summary>A credit note of only non-stock items posted no stock entry; there is then nothing to reverse.</summary>
    private async Task ReverseStockAsync(CreditNote note, DateOnly today, CancellationToken cancellationToken)
    {
        try
        {
            await stock.ReverseAsync(StockDocumentOf(note), today, cancellationToken);
        }
        catch (ConflictException ex) when (ex.Code == StockMessages.DocumentNotPosted)
        {
            // Stock R2: lines of items that are not stock managed post nothing, so such a credit note has no stock to take back.
        }
    }

    private static StockDocument StockDocumentOf(CreditNote note) => new(CreditNote.DocumentType, note.Id, note.Number);
}
