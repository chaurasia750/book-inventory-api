using BookInventory.Application.Sales;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Models;
using BookInventory.BuildingBlocks.Persistence.Contracts;
using BookInventory.BuildingBlocks.Persistence.Extensions;
using BookInventory.Domain.Sales;
using Microsoft.EntityFrameworkCore;

namespace BookInventory.Infrastructure.Sales;

/// <summary>The data side of the sales use cases (spec sales R6, R15); every query is limited to the session's company.</summary>
[AutoRegister(ServiceLifetime.Scoped, typeof(ISalesStore))]
public sealed class SalesStore(IUnitOfWork unitOfWork) : ISalesStore
{
    private const int MaxPageSize = 200;

    private readonly IRepository<SchoolOrder> _orders = unitOfWork.Repository<SchoolOrder>();
    private readonly IRepository<Invoice> _invoices = unitOfWork.Repository<Invoice>();

    public Task AddAsync(SchoolOrder order, CancellationToken cancellationToken = default) => _orders.AddAsync(order, cancellationToken);

    public void Remove(SchoolOrder order) => _orders.Remove(order);

    public Task AddAsync(Invoice invoice, CancellationToken cancellationToken = default) => _invoices.AddAsync(invoice, cancellationToken);

    public Task<Invoice?> FindInvoiceForManagementAsync(Guid id, CancellationToken cancellationToken = default) =>
        _invoices.Table.Include(i => i.Lines).FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

    public Task<Invoice?> FindInvoiceForReadAsync(Guid id, CancellationToken cancellationToken = default) =>
        _invoices.TableNoTracking.Include(i => i.Lines).FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

    public async Task<IReadOnlyList<InvoiceSummary>> InvoicesOfOrderAsync(Guid orderId, CancellationToken cancellationToken = default) =>
        await InvoiceRows(_invoices.TableNoTracking.Where(i => i.SchoolOrderId == orderId)).ToListAsync(cancellationToken);

    public async Task<IPagedList<InvoiceSummary>> SearchInvoicesAsync(InvoiceQuery query, CancellationToken cancellationToken = default)
    {
        var search = query.Search?.Trim().ToLowerInvariant();
        var year = query.FinancialYear?.Trim();
        var rows = _invoices.TableNoTracking
            .Where(i => query.SchoolId == null || i.SchoolId == query.SchoolId)
            .Where(i => query.Status == null || i.Status == query.Status)
            .Where(i => string.IsNullOrEmpty(year) || i.FinancialYear == year)
            .Where(i => string.IsNullOrEmpty(search)
                        || i.Number.ToLower().Contains(search) || i.OrderNumber.ToLower().Contains(search)
                        || i.SchoolName.ToLower().Contains(search) || i.SchoolCode.ToLower().Contains(search));
        return await InvoiceRows(rows).ToPagedListAsync(query.PageIndex < 1 ? 1 : query.PageIndex, Math.Clamp(query.PageSize < 1 ? 20 : query.PageSize, 1, MaxPageSize), cancellationToken);
    }

    private static IQueryable<InvoiceSummary> InvoiceRows(IQueryable<Invoice> invoices) =>
        invoices
            .OrderByDescending(i => i.InvoiceDate).ThenByDescending(i => i.CreatedAt)
            .Select(i => new InvoiceSummary(i.Id, i.Number, i.InvoiceDate, i.SchoolOrderId, i.OrderNumber, i.SchoolId, i.SchoolCode, i.SchoolName, i.Status, i.Totals.GrandTotal));

    public Task<SchoolOrder?> FindOrderForManagementAsync(Guid id, CancellationToken cancellationToken = default) =>
        _orders.Table.Include(o => o.Lines).FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

    public Task<SchoolOrder?> FindOrderForRemovalAsync(Guid id, CancellationToken cancellationToken = default) =>
        _orders.Table.FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

    public Task<SchoolOrder?> FindOrderForReadAsync(Guid id, CancellationToken cancellationToken = default) =>
        _orders.TableNoTracking.Include(o => o.Lines).FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

    public async Task<IPagedList<SchoolOrderSummary>> SearchOrdersAsync(SchoolOrderQuery query, CancellationToken cancellationToken = default)
    {
        var search = query.Search?.Trim().ToLowerInvariant();
        var session = query.Session?.Trim();
        var rows = _orders.TableNoTracking
            .Where(o => query.SchoolId == null || o.SchoolId == query.SchoolId)
            .Where(o => query.Status == null || o.Status == query.Status)
            .Where(o => string.IsNullOrEmpty(session) || o.Session == session)
            .Where(o => string.IsNullOrEmpty(search)
                        || o.Number.ToLower().Contains(search) || o.SchoolName.ToLower().Contains(search) || o.SchoolCode.ToLower().Contains(search))
            .OrderByDescending(o => o.OrderDate).ThenByDescending(o => o.CreatedAt)
            .Select(o => new SchoolOrderSummary(o.Id, o.Number, o.SchoolId, o.SchoolCode, o.SchoolName, o.OrderDate, o.Session, o.Status, o.Totals.GrandTotal));

        return await rows.ToPagedListAsync(query.PageIndex < 1 ? 1 : query.PageIndex, Math.Clamp(query.PageSize < 1 ? 20 : query.PageSize, 1, MaxPageSize), cancellationToken);
    }
}
