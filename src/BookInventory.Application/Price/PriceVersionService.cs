using BookInventory.Application.Item;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.Domain.Price;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Application.Price;

/// <summary>
/// R1-R3, R8: adds, edits and copies price versions. Every value rule is the aggregate's; this class checks what needs another
/// module (the item, through the item directory, item R16) or another version (a free name), calls one behaviour and lets the
/// request transaction commit. A version or item of another company is not found (P19).
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class PriceVersionService(IPriceStore store, IItemDirectory items, PriceCompanyGuard companyGuard)
{
    public async Task<PriceVersionResponse> AddAsync(Guid itemId, AddPriceVersionRequest request, CancellationToken cancellationToken)
    {
        companyGuard.Require();
        await RequireItemAsync(itemId, cancellationToken);
        await RequireFreeNameAsync(itemId, request.VersionName, excludingId: null, cancellationToken);

        var version = PriceVersion.Create(itemId, request.ToDetails());
        await store.AddAsync(version, cancellationToken);
        return PriceResponses.Of(version, isDefault: false, includePurchase: true);
    }

    public async Task EditAsync(Guid itemId, Guid versionId, EditPriceVersionRequest request, CancellationToken cancellationToken)
    {
        companyGuard.Require();
        var version = await store.FindForManagementAsync(versionId, cancellationToken);
        if (version is null || version.ItemId != itemId)
            throw new NotFoundException(PriceMessages.VersionNotFound);

        await RequireFreeNameAsync(itemId, request.VersionName, version.Id, cancellationToken);
        version.Edit(request.ToDetails());
    }

    /// <summary>R8: copies the source version of every selected item that has it and lacks the target; the rest are counted, never overwritten.</summary>
    public async Task<CopyPriceResult> CopyAsync(CopyPriceVersionRequest request, CancellationToken cancellationToken)
    {
        companyGuard.Require();
        var from = PriceVersion.NormalizeName(request.FromVersionName);
        var to = PriceVersion.NormalizeName(request.ToVersionName);
        var itemIds = await items.AllIdsAsync((request.Selection ?? new PriceSelection()).ToItemSelection(), cancellationToken);

        var sources = (await store.VersionsNamedAsync(itemIds, from, cancellationToken)).ToDictionary(v => v.ItemId);
        var existing = (await store.VersionsNamedAsync(itemIds, to, cancellationToken)).Select(v => v.ItemId).ToHashSet();

        var copies = sources.Values
            .Where(source => !existing.Contains(source.ItemId))
            .Select(source => PriceVersion.Create(
                source.ItemId,
                new PriceDetails(request.ToVersionName, request.EffectiveFrom!.Value, source.MrpAmount, source.SaleAmount, source.PurchaseAmount)))
            .ToList();
        await store.AddRangeAsync(copies, cancellationToken);

        return new CopyPriceResult(
            itemIds.Count, copies.Count, itemIds.Count - sources.Count, sources.Values.Count(s => existing.Contains(s.ItemId)));
    }

    private async Task RequireItemAsync(Guid itemId, CancellationToken cancellationToken)
    {
        if (await items.GetAsync(itemId, cancellationToken) is null)
            throw new NotFoundException(PriceMessages.ItemNotFound);
    }

    private async Task RequireFreeNameAsync(Guid itemId, string versionName, Guid? excludingId, CancellationToken cancellationToken)
    {
        if (await store.IsNameTakenAsync(itemId, PriceVersion.NormalizeName(versionName), excludingId, cancellationToken))
            throw new ConflictException(PriceMessages.NameTaken);
    }
}
