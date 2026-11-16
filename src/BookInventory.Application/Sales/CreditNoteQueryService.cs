using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.BuildingBlocks.Domain.Models;
using BookInventory.Domain.Sales;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Application.Sales;

/// <summary>R54, R61: the credit note list and detail and the returns of an invoice. The company comes from the platform filter; the roles are checked on the controller.</summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class CreditNoteQueryService(ICreditNoteStore notes, ISalesStore sales)
{
    public async Task<IPagedList<CreditNoteSummary>> SearchAsync(CreditNoteSearchQuery query, CancellationToken cancellationToken)
    {
        var page = await notes.SearchAsync(query, cancellationToken);
        return new PagedList<CreditNoteSummary>
        {
            PageIndex = page.PageIndex,
            PageSize = page.PageSize,
            IndexFrom = page.IndexFrom,
            TotalCount = page.TotalCount,
            TotalPages = page.TotalPages,
            Items = [.. page.Items.Select(Summary)]
        };
    }

    public async Task<CreditNoteResponse> GetAsync(Guid id, CancellationToken cancellationToken) =>
        Detail(await notes.FindForReadAsync(id, cancellationToken) ?? throw new NotFoundException(ReturnMessages.CreditNoteNotFound));

    /// <summary>R54, R61: what each line of the invoice sold, what live credit notes returned and what is left, and the credit notes of the invoice.</summary>
    public async Task<InvoiceReturns> OfInvoiceAsync(Guid invoiceId, CancellationToken cancellationToken)
    {
        var invoice = await sales.FindInvoiceForReadAsync(invoiceId, cancellationToken) ?? throw new NotFoundException(SalesMessages.InvoiceNotFound);
        var returned = await notes.ReturnedQuantitiesAsync(invoiceId, cancellationToken);
        var lines = invoice.Lines.Select(l => new ReturnableLine(l.Id, l.ItemId, l.ItemCode, l.ItemName, l.ClassName, l.RateAmount, l.Quantity, returned.GetValueOrDefault(l.Id)));
        var credit = await notes.OfInvoiceAsync(invoiceId, cancellationToken);
        return new InvoiceReturns(invoice.Id, invoice.Number, [.. lines], [.. credit.Select(Summary)]);
    }

    public static CreditNoteSummary Summary(CreditNote n) =>
        new(n.Id, n.Number, n.NoteDate, n.SchoolId, n.SchoolName, n.SchoolCode, n.InvoiceNumber, n.Reason, n.Totals.GrandTotal, n.Status, n.CreatedAt);

    private static CreditNoteResponse Detail(CreditNote n) =>
        new(
            n.Id, n.Number, n.NoteDate, n.FinancialYear, n.SchoolId, n.SchoolCode, n.SchoolName, n.InvoiceId, n.InvoiceNumber, n.Session, n.IsGstApplied,
            n.Reason, n.Remark, n.Totals.Subtotal, n.Totals.TotalDiscount, n.Totals.Taxable, n.Totals.Cgst, n.Totals.Sgst, n.Totals.Igst,
            n.Totals.RoundOff, n.Totals.GrandTotal, n.Status, n.CancelReason, n.CancelledOn, [.. n.Lines.Select(Line)], n.CreatedAt);

    private static CreditNoteLineResponse Line(CreditNoteLine l) =>
        new(
            l.ItemId, l.ItemCode, l.ItemName, l.PriceVersionName, l.MrpAmount, l.RateAmount, l.Quantity, l.Condition, l.ItemDiscountKind,
            l.ItemDiscountValue, l.GrossAmount, l.ItemDiscountAmount, l.LineAmount, l.TotalDiscountShareAmount, l.TaxableAmount,
            l.CgstAmount, l.SgstAmount, l.IgstAmount);
}
