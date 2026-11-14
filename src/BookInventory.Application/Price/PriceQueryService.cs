using BookInventory.Application.Item;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.BuildingBlocks.Domain.Models;
using BookInventory.Domain.Price;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Application.Price;

/// <summary>
/// R4, R5, R9: an item's versions and the price list. The default version is picked by the one rule of
/// <see cref="DefaultPrice"/> against the business date. Staff get the same answer without the purchase price: it is left out
/// in the projection, so it cannot leak by accident. Items are found through the item directory (item R16).
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class PriceQueryService(IPriceStore store, IItemDirectory items, BusinessCalendar calendar, ICurrentUser currentUser)
{
    private const string StaffRole = "STAFF";

    public async Task<IReadOnlyList<PriceVersionResponse>> VersionsOfItemAsync(Guid itemId, CancellationToken cancellationToken)
    {
        var item = await items.GetAsync(itemId, cancellationToken);
        if (item is null || (IsStaff && !item.IsActive))
            throw new NotFoundException(PriceMessages.ItemNotFound);

        var versions = await store.VersionsOfItemsAsync([itemId], cancellationToken);
        return Project(versions);
    }

    public async Task<IPagedList<PriceListRow>> ListAsync(PriceListQuery query, CancellationToken cancellationToken)
    {
        var request = new ItemPageRequest(
            query.Search, new ItemSelection(query.VendorId, query.Category, query.ClassName, OnlyActive: IsStaff), ItemOrder(query.OrderBy), query.PageIndex, query.PageSize);
        var page = await items.PageAsync(request, cancellationToken);

        var references = await items.ReferencesAsync(page.Ids, cancellationToken);
        var byItem = (await store.VersionsOfItemsAsync(page.Ids, cancellationToken)).GroupBy(v => v.ItemId).ToDictionary(g => g.Key, g => Project([.. g]));

        return new PagedList<PriceListRow>
        {
            PageIndex = query.PageIndex < 1 ? 1 : query.PageIndex,
            PageSize = Math.Clamp(query.PageSize < 1 ? 20 : query.PageSize, 1, 200),
            IndexFrom = 1,
            TotalCount = page.TotalCount,
            TotalPages = (int)Math.Ceiling(page.TotalCount / (double)Math.Clamp(query.PageSize < 1 ? 20 : query.PageSize, 1, 200)),
            Items =
            [
                .. page.Ids.Where(references.ContainsKey).Select(id => new PriceListRow(
                    id, references[id].Code, references[id].Name, byItem.GetValueOrDefault(id)?.FirstOrDefault(v => v.IsDefault)))
            ]
        };
    }

    /// <summary>One item's versions newest first with the default marked; the purchase price only for the Owner.</summary>
    private List<PriceVersionResponse> Project(IReadOnlyCollection<PriceVersion> versions)
    {
        var defaultId = DefaultPrice.IdOf(versions.Select(v => new VersionStamp(v.Id, v.EffectiveFrom, v.CreatedAt)), calendar.Today());
        return
        [
            .. versions
                .OrderByDescending(v => v.EffectiveFrom)
                .ThenByDescending(v => v.CreatedAt)
                .Select(v => PriceResponses.Of(v, v.Id == defaultId, includePurchase: !IsStaff))
        ];
    }

    /// <summary>The price list sorts by the item's name or code; anything else falls back to the name.</summary>
    private static string ItemOrder(string? orderBy)
    {
        var parts = (orderBy ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var column = parts.Length > 0 && parts[0].Equals("ItemCode", StringComparison.OrdinalIgnoreCase) ? "Code" : "Name";
        var descending = parts.Length > 1 && parts[1].Equals("DESC", StringComparison.OrdinalIgnoreCase);
        return $"{column} {(descending ? "DESC" : "ASC")}";
    }

    private bool IsStaff => currentUser.Role == StaffRole;
}

/// <summary>The one projection of a version into its response, used by the add and the read use cases.</summary>
internal static class PriceResponses
{
    public static PriceVersionResponse Of(PriceVersion version, bool isDefault, bool includePurchase) =>
        new(version.Id, version.ItemId, version.VersionName, version.EffectiveFrom, version.MrpAmount, version.SaleAmount,
            includePurchase ? version.PurchaseAmount : null, isDefault);
}
