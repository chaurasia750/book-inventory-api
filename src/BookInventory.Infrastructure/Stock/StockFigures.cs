using BookInventory.Application.Item;
using BookInventory.Application.Stock;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Persistence.Contracts;
using BookInventory.Domain.Stock;
using Microsoft.EntityFrameworkCore;

namespace BookInventory.Infrastructure.Stock;

/// <summary>
/// The data side of the dashboard figures of stock (spec report R10): the active stock-managed items come from the item directory
/// (item R16); their stock cards are read in grouped queries limited to the session's company by the platform filter. An item with
/// no stock card has zero Main stock, so it is at or below zero.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped, typeof(IStockFigures))]
public sealed class StockFigures(IUnitOfWork unitOfWork, IItemDirectory items) : IStockFigures
{
    private static readonly ItemSelection Tracked = new(OnlyActive: true, StockManaged: true);

    private readonly IRepository<ItemStock> _stocks = unitOfWork.Repository<ItemStock>();

    public async Task<StockCounts> CountsAsync(CancellationToken cancellationToken = default)
    {
        var ids = await items.AllIdsAsync(Tracked, cancellationToken);
        var cards = await _stocks.TableNoTracking
            .Where(s => ids.Contains(s.ItemId))
            .Select(s => new { s.MainQuantity, s.DamagedQuantity })
            .ToListAsync(cancellationToken);

        var aboveZero = cards.Count(c => c.MainQuantity > 0);
        return new StockCounts(ids.Count - aboveZero, cards.Count(c => c.MainQuantity < 0), cards.Where(c => c.DamagedQuantity > 0).Sum(c => c.DamagedQuantity));
    }

    public async Task<MainQuantityPage> MainQuantitiesAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = default)
    {
        var size = Math.Clamp(pageSize, 1, IStockFigures.MaxPageSize);
        var ids = await items.AllIdsAsync(Tracked, cancellationToken);
        var query = _stocks.TableNoTracking.Where(s => ids.Contains(s.ItemId) && s.MainQuantity > 0);
        var total = await query.CountAsync(cancellationToken);
        var page = await query
            .OrderBy(s => s.ItemId)
            .Skip((Math.Max(pageIndex, 1) - 1) * size)
            .Take(size)
            .Select(s => new MainQuantity(s.ItemId, s.MainQuantity))
            .ToListAsync(cancellationToken);
        return new MainQuantityPage(page, total);
    }
}
