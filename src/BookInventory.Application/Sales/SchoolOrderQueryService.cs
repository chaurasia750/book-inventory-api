using BookInventory.Application.Stock;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.BuildingBlocks.Domain.Models;
using BookInventory.Domain.Sales;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Application.Sales;

/// <summary>R5, R15: the order list and one order with its lines, remaining quantities and the low-stock warning (stock R20).</summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class SchoolOrderQueryService(ISalesStore store, IStockQuantities stock)
{
    public Task<IPagedList<SchoolOrderSummary>> ListAsync(SchoolOrderQuery query, CancellationToken cancellationToken) =>
        store.SearchOrdersAsync(query, cancellationToken);

    public async Task<SchoolOrderDetail> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var order = await store.FindOrderForReadAsync(id, cancellationToken) ?? throw new NotFoundException(SalesMessages.OrderNotFound);
        return await DetailOfAsync(order, cancellationToken);
    }

    /// <summary>The detail of an order in hand (also one created in this request, before it is saved).</summary>
    public async Task<SchoolOrderDetail> DetailOfAsync(SchoolOrder order, CancellationToken cancellationToken)
    {
        var quantities = await stock.OfItemsAsync([.. order.Lines.Select(l => l.ItemId)], cancellationToken);

        return new SchoolOrderDetail
        {
            Id = order.Id,
            Number = order.Number,
            School = new SaleSchool(order.SchoolId, order.SchoolCode, order.SchoolName, order.SchoolState),
            OrderDate = order.OrderDate,
            Session = order.Session,
            IsGstApplied = order.IsGstApplied,
            GstMode = order.GstMode,
            TotalDiscount = order.TotalDiscount,
            Remark = order.Remark,
            Status = order.Status,
            CloseReason = order.CloseReason,
            Totals = order.Totals,
            Lines = [.. order.Lines.Select(l => ViewOf(l, quantities.GetValueOrDefault(l.ItemId)?.MainQuantity))],
            Invoices = await store.InvoicesOfOrderAsync(order.Id, cancellationToken)
        };
    }

    /// <summary>R5: a line whose Main stock is below what is still to deliver shows the warning; it never blocks.</summary>
    private static SchoolOrderLineView ViewOf(SchoolOrderLine l, int? mainStock) =>
        new(
            l.Id, l.Item, l.Price, l.Quantity, l.ItemDiscount,
            new SaleLineAmounts(l.GrossAmount, l.ItemDiscountAmount, l.LineAmount, l.TotalDiscountShareAmount, l.TaxableAmount, l.CgstAmount, l.SgstAmount, l.IgstAmount),
            l.InvoicedQuantity, l.RemainingQuantity, mainStock, mainStock is { } main && main < l.RemainingQuantity);
}
