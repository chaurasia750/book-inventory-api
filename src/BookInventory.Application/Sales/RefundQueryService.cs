using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.BuildingBlocks.Domain.Models;
using BookInventory.Domain.Sales;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Application.Sales;

/// <summary>R69: the refund list and detail. The company comes from the platform filter; the roles are checked on the controller.</summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class RefundQueryService(IRefundStore refunds)
{
    public async Task<IPagedList<RefundResponse>> SearchAsync(RefundSearchQuery query, CancellationToken cancellationToken)
    {
        var page = await refunds.SearchAsync(query, cancellationToken);
        return new PagedList<RefundResponse>
        {
            PageIndex = page.PageIndex,
            PageSize = page.PageSize,
            IndexFrom = page.IndexFrom,
            TotalCount = page.TotalCount,
            TotalPages = page.TotalPages,
            Items = [.. page.Items.Select(Response)]
        };
    }

    public async Task<RefundResponse> GetAsync(Guid id, CancellationToken cancellationToken) =>
        Response(await refunds.FindForReadAsync(id, cancellationToken) ?? throw new NotFoundException(ReturnMessages.RefundNotFound));

    private static RefundResponse Response(SchoolRefund r) =>
        new(
            r.Id, r.Number, r.RefundDate, r.FinancialYear, r.SchoolId, r.SchoolCode, r.SchoolName, r.CreditNoteId, r.CreditNoteNumber, r.Mode,
            r.Reference, r.Amount, r.Remark, r.Status, r.CancelReason, r.CancelledOn, r.CreatedAt);
}
