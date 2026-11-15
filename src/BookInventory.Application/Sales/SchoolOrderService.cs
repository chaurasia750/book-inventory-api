using BookInventory.Application.Core;
using BookInventory.Application.Price;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.Domain.Sales;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Application.Sales;

/// <summary>
/// R2, R6, R14: creates, replaces, deletes, confirms and closes school orders. The order number comes from the company code series
/// of the order's session (owner's answer Q1); a draft that moves to another session gets a number of that session.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class SchoolOrderService(
    ISalesStore store, OrderComposer composer, ICompanyCodes codes, BusinessCalendar calendar, PriceCompanyGuard companyGuard)
{
    public async Task<SchoolOrder> CreateAsync(SaveSchoolOrderRequest request, CancellationToken cancellationToken)
    {
        companyGuard.Require();
        var today = calendar.Today();
        var orderDate = request.OrderDate ?? today;
        var header = await composer.HeaderAsync(request, orderDate, cancellationToken);
        var lines = await composer.LinesAsync(new OrderLinesRequest(request.SchoolId!.Value, orderDate, request.Lines ?? []), cancellationToken);

        var number = SchoolOrder.NumberOf(header.Session, await codes.NextNumberAsync(SchoolOrder.NumberSeries(header.Session), cancellationToken));
        var order = SchoolOrder.Draft(number, header, lines, today);
        await store.AddAsync(order, cancellationToken);
        return order;
    }

    public async Task ReplaceAsync(Guid id, SaveSchoolOrderRequest request, CancellationToken cancellationToken)
    {
        companyGuard.Require();
        var order = await FindAsync(id, cancellationToken);
        if (order.Status == SchoolOrderStatus.Confirmed)
        {
            await AmendAsync(order, request, cancellationToken);
            return;
        }

        order.RequireDraft();
        var today = calendar.Today();
        var orderDate = request.OrderDate ?? order.OrderDate;
        var header = await composer.HeaderAsync(request, orderDate, cancellationToken);
        var lines = await composer.LinesAsync(new OrderLinesRequest(request.SchoolId!.Value, orderDate, request.Lines ?? []), cancellationToken);

        var newNumber = header.Session == order.Session
            ? null
            : SchoolOrder.NumberOf(header.Session, await codes.NextNumberAsync(SchoolOrder.NumberSeries(header.Session), cancellationToken));
        order.Replace(header, lines, today, newNumber);
    }

    /// <summary>R20: a confirmed order keeps its school and date; its lines, discounts and remark change within the order's limits.</summary>
    private async Task AmendAsync(SchoolOrder order, SaveSchoolOrderRequest request, CancellationToken cancellationToken)
    {
        // R20, R34: what the request leaves out stays as the order has it (it is not reset to the school's defaults).
        var kept = request with { IsGstApplied = request.IsGstApplied ?? order.IsGstApplied, PaymentType = request.PaymentType ?? order.PaymentType };
        var header = await composer.HeaderAsync(kept, order.OrderDate, cancellationToken);
        var lines = await composer.LinesAsync(new OrderLinesRequest(order.SchoolId, order.OrderDate, request.Lines ?? []), cancellationToken);
        order.Amend(header, lines);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        companyGuard.Require();
        var order = await FindAsync(id, cancellationToken);
        order.RequireDraft();
        store.Remove(order);
    }

    public async Task ConfirmAsync(Guid id, CancellationToken cancellationToken)
    {
        companyGuard.Require();
        (await FindAsync(id, cancellationToken)).Confirm();
    }

    public async Task CloseAsync(Guid id, CloseSchoolOrderRequest request, CancellationToken cancellationToken)
    {
        companyGuard.Require();
        (await FindAsync(id, cancellationToken)).Close(request.Reason);
    }

    private async Task<SchoolOrder> FindAsync(Guid id, CancellationToken cancellationToken) =>
        await store.FindOrderForManagementAsync(id, cancellationToken) ?? throw new NotFoundException(SalesMessages.OrderNotFound);
}
