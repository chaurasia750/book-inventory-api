using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.BuildingBlocks.Domain.Models;
using BookInventory.Domain.Sales;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Application.Sales;

/// <summary>R49: the counter bill list and detail. The company comes from the platform filter; the roles are checked on the controller.</summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class CounterBillQueryService(ICounterBillStore bills, ICounterStore counters)
{
    public async Task<IPagedList<CounterBillSummary>> SearchAsync(CounterBillSearchQuery query, CancellationToken cancellationToken)
    {
        var page = await bills.SearchAsync(query, cancellationToken);
        return new PagedList<CounterBillSummary>
        {
            PageIndex = page.PageIndex,
            PageSize = page.PageSize,
            IndexFrom = page.IndexFrom,
            TotalCount = page.TotalCount,
            TotalPages = page.TotalPages,
            Items = [.. page.Items.Select(Summary)]
        };
    }

    public async Task<CounterBillResponse> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var bill = await bills.FindForReadAsync(id, cancellationToken) ?? throw new NotFoundException(CounterMessages.BillNotFound);
        var counter = await counters.FindCounterForReadAsync(bill.CounterId, cancellationToken);
        return Detail(bill, counter?.Name);
    }

    private static CounterBillSummary Summary(CounterBill b) =>
        new(b.Id, b.Number, b.BillDate, b.CounterId, b.SchoolId, b.SchoolName, b.StudentId, b.StudentName, b.StudentClass, b.Totals.GrandTotal, b.Status, b.CreatedAt);

    private static CounterBillResponse Detail(CounterBill b, string? counterName) =>
        new(
            b.Id, b.Number, b.BillDate, b.FinancialYear, b.CounterId, counterName, b.SchoolId, b.SchoolCode, b.SchoolName, b.Session,
            new StudentSnapshot(b.StudentId, b.StudentName, b.StudentClass, b.StudentSection, b.StudentRollNumber, b.StudentParentMobile),
            b.IsGstApplied, b.TotalDiscountKind, b.TotalDiscountValue, b.Totals.Subtotal, b.Totals.TotalDiscount, b.Totals.Taxable, b.Totals.Cgst,
            b.Totals.Sgst, b.Totals.Igst, b.Totals.RoundOff, b.Totals.GrandTotal, b.Remark, b.Status, b.CancelReason, b.CancelledOn,
            [.. b.Lines.Select(Line)], [.. b.Payments.Select(p => new CounterBillPaymentResponse(p.Mode, p.Amount, p.Reference))], b.CreatedAt);

    private static CounterBillLineResponse Line(CounterBillLine l) =>
        new(
            l.ItemId, l.ItemCode, l.ItemName, l.Category, l.PriceVersionName, l.MrpAmount, l.Quantity, l.ItemDiscountKind, l.ItemDiscountValue,
            l.GrossAmount, l.ItemDiscountAmount, l.LineAmount, l.TaxableAmount, l.CgstAmount, l.SgstAmount, l.IgstAmount);
}
