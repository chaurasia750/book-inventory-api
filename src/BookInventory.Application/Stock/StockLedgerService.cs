using BookInventory.Application.Price;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.Domain.Stock;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Application.Stock;

/// <summary>
/// R7, R8, R12: the Owner's opening stock and adjustment. Every quantity rule is the aggregate's; this class checks what needs
/// another module (the item, item R16/R17) or the stored history (an existing opening), calls one behaviour and lets the request
/// transaction commit. An item of another company is not found (P19).
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class StockLedgerService(
    IStockStore store, StockCards cards, StockManagedItems items, PriceCompanyGuard companyGuard, BusinessCalendar calendar)
{
    public async Task<StockPostedResponse> EnterOpeningAsync(Guid itemId, EnterOpeningRequest request, CancellationToken cancellationToken)
    {
        companyGuard.Require();
        await items.RequireAsync(itemId, cancellationToken);
        var stockType = request.StockType!.Value;
        if ((await store.OpeningsOfAsync([itemId], cancellationToken)).Contains(new StockKey(itemId, stockType)))
            throw new ConflictException(StockMessages.OpeningExists);

        var card = await cards.ForItemAsync(itemId, cancellationToken);
        var entry = card.EnterOpening(stockType, request.Quantity!.Value, request.OpeningDate!.Value);
        return PostedOf(card, entry);
    }

    public async Task<StockPostedResponse> AdjustAsync(Guid itemId, AdjustStockRequest request, CancellationToken cancellationToken)
    {
        companyGuard.Require();
        await items.RequireAsync(itemId, cancellationToken);

        var card = await cards.ForItemAsync(itemId, cancellationToken);
        var entry = card.Adjust(request.StockType!.Value, request.CountedQuantity!.Value, request.Reason, calendar.Today());
        return PostedOf(card, entry);
    }

    private static StockPostedResponse PostedOf(ItemStock card, StockEntry entry) =>
        new(entry.Id, card.ItemId, entry.StockType, entry.MovementKind, entry.QuantityIn, entry.QuantityOut, card.QuantityOf(entry.StockType));
}
