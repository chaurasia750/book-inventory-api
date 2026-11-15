using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.BuildingBlocks.Domain.Models;
using BookInventory.Domain.Sales;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Application.Sales;

/// <summary>R13, R15: the invoice list and one invoice with its snapshot, lines and totals.</summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class InvoiceQueryService(ISalesStore store, IInvoiceLayoutStore layouts)
{
    public Task<IPagedList<InvoiceSummary>> ListAsync(InvoiceQuery query, CancellationToken cancellationToken) => store.SearchInvoicesAsync(query, cancellationToken);

    /// <summary>R13, R26: the invoice, its groups shown in <paramref name="layout"/> or, when none is asked, in the school's default.</summary>
    public async Task<InvoiceDetail> GetAsync(Guid id, InvoiceLayout? layout, CancellationToken cancellationToken) =>
        (await PrintableAsync(id, layout, cancellationToken)).Detail;

    /// <summary>R28, R33: the invoice together with the company identity it kept (for the PDF).</summary>
    public async Task<PrintableInvoice> PrintableAsync(Guid id, InvoiceLayout? layout, CancellationToken cancellationToken)
    {
        var i = await store.FindInvoiceForReadAsync(id, cancellationToken) ?? throw new NotFoundException(SalesMessages.InvoiceNotFound);
        var chosen = layout ?? (await layouts.FindForReadAsync(i.SchoolId, cancellationToken))?.Layout ?? SchoolInvoiceLayout.Default;
        return new PrintableInvoice(DetailOf(i, chosen), i.CompanySnapshot);
    }

    private static InvoiceDetail DetailOf(Invoice i, InvoiceLayout chosen)
    {
        var lines = ViewsOf(i);
        return new InvoiceDetail
        {
            Summary = new InvoiceSummary(i.Id, i.Number, i.InvoiceDate, i.SchoolOrderId, i.OrderNumber, i.SchoolId, i.SchoolCode, i.SchoolName, i.Status, i.Totals.GrandTotal),
            FinancialYear = i.FinancialYear,
            School = new SaleSchool(i.SchoolId, i.SchoolCode, i.SchoolName, i.SchoolState, i.SchoolAddress),
            SchoolGstin = i.SchoolGstin,
            CompanyState = i.CompanyState,
            Session = i.Session,
            IsGstApplied = i.IsGstApplied,
            GstMode = i.GstMode,
            TotalDiscount = new Discount(i.TotalDiscountKind, i.TotalDiscountValue),
            Totals = i.Totals,
            CancelReason = i.CancelReason,
            CancelledOn = i.CancelledOn,
            Lines = lines,
            Layout = chosen,
            Groups = InvoiceLayoutView.Of(chosen, lines),
            GstSummary = i.IsGstApplied ? GstRateSummary.Of(i.Lines) : [],
            AmountInWords = AmountInWords.Of(i.Totals.GrandTotal)
        };
    }

    private static IReadOnlyList<InvoiceLineView> ViewsOf(Invoice i) =>
        [.. i.Lines.Select(l => new InvoiceLineView(
            l.Id, l.Delivery.Item, l.Delivery.Price, l.Quantity, l.ItemDiscount,
            new SaleLineAmounts(l.GrossAmount, l.ItemDiscountAmount, l.LineAmount, l.TotalDiscountShareAmount, l.TaxableAmount, l.CgstAmount, l.SgstAmount, l.IgstAmount),
            l.ClassName))];
}
