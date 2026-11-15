using BookInventory.Application.Planning;
using BookInventory.Application.Price;
using BookInventory.Application.School;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.Domain.Sales;
using BookInventory.Domain.School;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Application.Sales;

/// <summary>
/// R22, R23: adds many lines to a draft or a confirmed order at once: the books of whole class sets (students x quantity per
/// student, from the planning lists of the order's session) or many chosen items with one quantity and class. The lines are made
/// by the composer at each item's default price version; what cannot be added is reported, never silently dropped.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class OrderLineAdditionService(
    ISalesStore store, OrderComposer composer, IClassBookLists bookLists, ISchoolDirectory schools, PriceCompanyGuard companyGuard)
{
    public async Task<AddLinesResponse> AddClassSetsAsync(Guid orderId, AddClassSetsRequest request, CancellationToken cancellationToken)
    {
        companyGuard.Require();
        var order = await FindAsync(orderId, cancellationToken);
        var lists = await bookLists.ListsOfAsync(order.SchoolId, order.Session, cancellationToken);
        var lines = new List<SchoolOrderLineRequest>();
        var leftOut = new List<LeftOutLine>();
        foreach (var set in request.Classes ?? [])
        {
            var className = await schools.ClassOfAsync(order.SchoolId, set.ClassName, cancellationToken)
                            ?? throw new ValidationFailureException(SalesMessages.InvalidClass) { Subject = set.ClassName };
            if (!lists.TryGetValue(SchoolClass.KeyOf(className)!, out var list))
                leftOut.Add(new LeftOutLine(null, null, className, SalesMessages.NoBookList));
            else
                lines.AddRange(list.Books.Select(b => new SchoolOrderLineRequest(b.ItemId, null, b.QuantityPerStudent * set.Students, null, className)));
        }

        return await AddAsync(order, lines, leftOut, cancellationToken);
    }

    public async Task<AddLinesResponse> AddBulkAsync(Guid orderId, AddBulkLinesRequest request, CancellationToken cancellationToken)
    {
        companyGuard.Require();
        var order = await FindAsync(orderId, cancellationToken);
        var lines = (request.ItemIds ?? []).Distinct()
            .Select(id => new SchoolOrderLineRequest(id, null, request.Quantity, request.ItemDiscount, request.ClassName))
            .ToList();
        return await AddAsync(order, lines, [], cancellationToken);
    }

    private async Task<AddLinesResponse> AddAsync(SchoolOrder order, List<SchoolOrderLineRequest> lines, List<LeftOutLine> leftOut, CancellationToken cancellationToken)
    {
        var composed = await composer.TryLinesAsync(new OrderLinesRequest(order.SchoolId, order.OrderDate, lines), cancellationToken);
        order.AddLines(composed.Lines);
        return new AddLinesResponse(composed.Lines.Count, [.. leftOut, .. composed.LeftOut]);
    }

    private async Task<SchoolOrder> FindAsync(Guid id, CancellationToken cancellationToken) =>
        await store.FindOrderForManagementAsync(id, cancellationToken) ?? throw new NotFoundException(SalesMessages.OrderNotFound);
}
