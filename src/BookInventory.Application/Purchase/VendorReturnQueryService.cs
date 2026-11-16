using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.BuildingBlocks.Domain.Models;
using BookInventory.Domain.Purchase;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Application.Purchase;

/// <summary>R12: the vendor return list and detail. The company comes from the platform filter; the roles are checked on the controller.</summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class VendorReturnQueryService(IVendorReturnStore returns)
{
    public async Task<IPagedList<VendorReturnSummary>> SearchAsync(VendorReturnSearchQuery query, CancellationToken cancellationToken)
    {
        var page = await returns.SearchAsync(query, cancellationToken);
        return new PagedList<VendorReturnSummary>
        {
            PageIndex = page.PageIndex,
            PageSize = page.PageSize,
            IndexFrom = page.IndexFrom,
            TotalCount = page.TotalCount,
            TotalPages = page.TotalPages,
            Items = [.. page.Items.Select(Summary)]
        };
    }

    public async Task<VendorReturnResponse> GetAsync(Guid id, CancellationToken cancellationToken) =>
        Detail(await returns.FindForReadAsync(id, cancellationToken) ?? throw new NotFoundException(PurchaseMessages.VendorReturnNotFound));

    private static VendorReturnSummary Summary(VendorReturn r) =>
        new(r.Id, r.Number, r.ReturnDate, r.VendorId, r.VendorCode, r.VendorName, r.GrandTotalAmount, r.Status, r.CreatedAt);

    private static VendorReturnResponse Detail(VendorReturn r) =>
        new(
            r.Id, r.Number, r.ReturnDate, r.FinancialYear, r.VendorId, r.VendorCode, r.VendorName, r.Remark, r.SubtotalAmount, r.RoundOffAmount,
            r.GrandTotalAmount, r.Status, r.CancelReason, r.CancelledOn,
            [.. r.Lines.Select(l => new VendorReturnLineResponse(l.ItemId, l.ItemCode, l.ItemName, l.StockSource, l.RateAmount, l.Quantity, l.LineAmount))],
            r.CreatedAt);
}
