using BookInventory.Application.Core;
using BookInventory.Application.Price;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.Domain.Finance;
using BookInventory.Domain.Sales;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Application.Sales;

/// <summary>
/// R53-R58, R62: makes a credit note and cancels it. The invoice (when named) must be live and the school's own; the amounts are the
/// one rule of R7 shared as R55 says; the stock in and the school ledger credit are posted in the same transaction, and the returned
/// quantities go back to the order of the invoice; a cancel reverses all of it. The whole credit note fails when anything is refused.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class CreditNoteService(
    ICreditNoteStore notes, ISalesStore sales, CreditNoteComposer composer, ICompanyCodes codes, BusinessCalendar calendar, CreditNotePostings postings)
{
    public async Task<CreditNoteMade> MakeAsync(MakeCreditNoteRequest request, CancellationToken cancellationToken)
    {
        composer.RequireCompany();
        var invoice = await LiveInvoiceAsync(request, cancellationToken);
        var over = await OverReturnedAsync(invoice, request.Lines ?? [], cancellationToken);

        var today = calendar.Today();
        var noteDate = request.NoteDate ?? today;
        var financialYear = FinancialYear.Of(noteDate);
        var number = CreditNote.NumberOf(financialYear, await codes.NextNumberAsync(CreditNote.NumberSeries(financialYear), cancellationToken));
        var note = CreditNote.Make(await composer.ComposeAsync(request, new CreditNoteStamp(number, noteDate, today), invoice, cancellationToken));
        await notes.AddAsync(note, cancellationToken);

        await postings.PostAsync(note, cancellationToken);
        await GiveBackToOrderAsync(invoice, note, cancellationToken);
        return new CreditNoteMade(note.Id, note.Number, note.Totals.GrandTotal, over);
    }

    public async Task CancelAsync(Guid id, CancelCreditNoteRequest request, CancellationToken cancellationToken)
    {
        composer.RequireCompany();
        var note = await notes.FindForManagementAsync(id, cancellationToken) ?? throw new NotFoundException(ReturnMessages.CreditNoteNotFound);
        var today = calendar.Today();
        note.Cancel(request.Reason ?? string.Empty, today);

        await postings.ReverseAsync(note, today, cancellationToken);
        if (note.InvoiceId is not null)
        {
            var invoice = await sales.FindInvoiceForReadAsync(note.InvoiceId.Value, cancellationToken);
            var order = invoice is null ? null : await sales.FindOrderForManagementAsync(invoice.SchoolOrderId, cancellationToken);
            order?.TakeReturnBack(note.Delivery);
        }
    }

    /// <summary>R54: the invoice must exist, belong to the school and not be cancelled.</summary>
    private async Task<Invoice?> LiveInvoiceAsync(MakeCreditNoteRequest request, CancellationToken cancellationToken)
    {
        if (request.InvoiceId is null)
            return null;

        var invoice = await sales.FindInvoiceForReadAsync(request.InvoiceId.Value, cancellationToken) ?? throw new NotFoundException(SalesMessages.InvoiceNotFound);
        if (invoice.SchoolId != request.SchoolId)
            throw new ValidationFailureException(ReturnMessages.InvoiceOfOtherSchool);

        return invoice.Status == InvoiceStatus.Cancelled ? throw new ConflictException(ReturnMessages.InvoiceCancelled) : invoice;
    }

    /// <summary>R54 (FR-RET-06): the items whose invoice line is returned beyond what was sold, counting live credit notes; a warning, never a refusal.</summary>
    private async Task<IReadOnlyList<Guid>> OverReturnedAsync(Invoice? invoice, IReadOnlyList<CreditNoteLineRequest> lines, CancellationToken cancellationToken)
    {
        if (invoice is null)
            return [];

        var returned = await notes.ReturnedQuantitiesAsync(invoice.Id, cancellationToken);
        var asked = lines.Where(l => l.InvoiceLineId is not null).GroupBy(l => l.InvoiceLineId!.Value).ToDictionary(g => g.Key, g => g.Sum(l => l.Quantity ?? 0));
        return [.. invoice.Lines.Where(l => asked.TryGetValue(l.Id, out var now) && returned.GetValueOrDefault(l.Id) + now > l.Quantity).Select(l => l.ItemId).Distinct()];
    }

    /// <summary>R62: the returned quantities (and the rupee discounts they carried) are given back to the invoice's order, which may open again.</summary>
    private async Task GiveBackToOrderAsync(Invoice? invoice, CreditNote note, CancellationToken cancellationToken)
    {
        if (invoice is null)
            return;

        var order = await sales.FindOrderForManagementAsync(invoice.SchoolOrderId, cancellationToken) ?? throw new NotFoundException(SalesMessages.OrderNotFound);
        order.GiveBack(note.Delivery);
    }
}
