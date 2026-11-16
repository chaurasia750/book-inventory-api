using BookInventory.Application.Price;
using BookInventory.Application.Stock;
using BookInventory.BuildingBlocks.Application.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Application.Report;

/// <summary>R10: the stock card of the dashboard: the counts for everyone and, for the Owner, the value of the Main stock at today's purchase price (price R19).</summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class DashboardStockReader(IStockFigures stock, IPurchasePriceDirectory prices)
{
    public async Task<StockPart> ReadAsync(DashboardWindow window, bool withValue, CancellationToken cancellationToken)
    {
        var counts = await stock.CountsAsync(cancellationToken);
        if (!withValue)
            return new StockPart(counts.AtOrBelowZero, counts.BelowZero, counts.DamagedPieces, null, null);

        var (value, withoutPrice) = await ValueAsync(window.Today, cancellationToken);
        return new StockPart(counts.AtOrBelowZero, counts.BelowZero, counts.DamagedPieces, value, withoutPrice);
    }

    /// <summary>R10: the Main quantity above zero of every item times its purchase price today, a page of 500 items at a time; an item without a price counts as zero and is counted apart.</summary>
    private async Task<(decimal Value, int WithoutPrice)> ValueAsync(DateOnly today, CancellationToken cancellationToken)
    {
        decimal value = 0m;
        var withoutPrice = 0;
        var pageIndex = 1;
        MainQuantityPage page;
        do
        {
            page = await stock.MainQuantitiesAsync(pageIndex++, IStockFigures.MaxPageSize, cancellationToken);
            var priced = await prices.PurchasePricesAsync([.. page.Items.Select(i => i.ItemId)], today, cancellationToken);
            foreach (var item in page.Items)
            {
                if (priced.TryGetValue(item.ItemId, out var price))
                    value += item.Quantity * price.PurchaseAmount;
                else
                    withoutPrice++;
            }
        }
        while ((pageIndex - 1) * IStockFigures.MaxPageSize < page.TotalCount);
        return (value, withoutPrice);
    }
}
