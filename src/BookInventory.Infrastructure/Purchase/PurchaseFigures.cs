using BookInventory.Application.Purchase;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Persistence.Contracts;
using BookInventory.Domain.Purchase;
using Microsoft.EntityFrameworkCore;

namespace BookInventory.Infrastructure.Purchase;

/// <summary>The data side of the dashboard figures of purchase (spec report R8): one grouped query limited to the session's company by the platform filter.</summary>
[AutoRegister(ServiceLifetime.Scoped, typeof(IPurchaseFigures))]
public sealed class PurchaseFigures(IUnitOfWork unitOfWork) : IPurchaseFigures
{
    private readonly IRepository<VendorReturn> _returns = unitOfWork.Repository<VendorReturn>();

    public async Task<VendorReturnFigure> ReturnsAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        var figure = await _returns.TableNoTracking
            .Where(r => r.Status == VendorReturnStatus.Active && r.ReturnDate >= from && r.ReturnDate <= to)
            .GroupBy(_ => 1)
            .Select(g => new VendorReturnFigure(g.Count(), g.Sum(r => r.GrandTotalAmount)))
            .FirstOrDefaultAsync(cancellationToken);
        return figure ?? new VendorReturnFigure(0, 0m);
    }
}
