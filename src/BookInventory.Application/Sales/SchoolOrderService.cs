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
        var lines = await composer.LinesAsync(request.Lines ?? [], orderDate, cancellationToken);

        var number = SchoolOrder.NumberOf(header.Session, await codes.NextNumberAsync(SchoolOrder.NumberSeries(header.Session), cancellationToken));
        var order = SchoolOrder.Draft(number, header, lines, today);
        await store.AddAsync(order, cancellationToken);
        return order;
    }

    public async Task ReplaceAsync(Guid id, SaveSchoolOrderRequest request, CancellationToken cancellationToken)
    {
        companyGuard.Require();
        var order = await FindAsync(id, cancellationToken);
        order.RequireDraft();
        var today = calendar.Today();
        var orderDate = request.OrderDate ?? order.OrderDate;
        var header = await composer.HeaderAsync(request, orderDate, cancellationToken);
        var lines = await composer.LinesAsync(request.Lines ?? [], orderDate, cancellationToken);

        var newNumber = header.Session == order.Session
            ? null
            : SchoolOrder.NumberOf(header.Session, await codes.NextNumberAsync(SchoolOrder.NumberSeries(header.Session), cancellationToken));
        order.Replace(header, lines, today, newNumber);
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
