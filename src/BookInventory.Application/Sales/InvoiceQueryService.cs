using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.BuildingBlocks.Domain.Models;
using BookInventory.Domain.Sales;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Application.Sales;

/// <summary>R13, R15: the invoice list and one invoice with its snapshot, lines and totals.</summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class InvoiceQueryService(ISalesStore store)
{
    public Task<IPagedList<InvoiceSummary>> ListAsync(InvoiceQuery query, CancellationToken cancellationToken) => store.SearchInvoicesAsync(query, cancellationToken);

    public async Task<InvoiceDetail> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var i = await store.FindInvoiceForReadAsync(id, cancellationToken) ?? throw new NotFoundException(SalesMessages.InvoiceNotFound);
        return new InvoiceDetail
        {
            Summary = new InvoiceSummary(i.Id, i.Number, i.InvoiceDate, i.SchoolOrderId, i.OrderNumber, i.SchoolId, i.SchoolCode, i.SchoolName, i.Status, i.Totals.GrandTotal),
            FinancialYear = i.FinancialYear,
            School = new SaleSchool(i.SchoolId, i.SchoolCode, i.SchoolName, i.SchoolState),
            SchoolGstin = i.SchoolGstin,
            CompanyState = i.CompanyState,
            Session = i.Session,
            IsGstApplied = i.IsGstApplied,
            GstMode = i.GstMode,
            TotalDiscount = new Discount(i.TotalDiscountKind, i.TotalDiscountValue),
            Totals = i.Totals,
            CancelReason = i.CancelReason,
            CancelledOn = i.CancelledOn,
            Lines = [.. i.Lines.Select(l => new InvoiceLineView(
                l.Id, l.Delivery.Item, l.Delivery.Price, l.Quantity, l.ItemDiscount,
                new SaleLineAmounts(l.GrossAmount, l.ItemDiscountAmount, l.LineAmount, l.TotalDiscountShareAmount, l.TaxableAmount, l.CgstAmount, l.SgstAmount, l.IgstAmount)))]
        };
    }
}
