using BookInventory.Application.Item;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.Domain.Price;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Application.Price;

/// <summary>
/// R10-R14: the bulk price change. The selection becomes item ids through the item directory (item R16); the named version of
/// each is changed by the one calculation of <see cref="PriceChange"/>. Preview and apply share <see cref="PlanAsync"/>, so the
/// preview can never differ from what apply does; apply recomputes everything on the server and edits each version through its own
/// root in one request transaction, all or nothing (decision D4). A preview saves nothing.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class BulkPriceChangeService(IPriceStore store, IItemDirectory items, PriceCompanyGuard companyGuard)
{
    private const int MaxPreviewPageSize = 200;

    public async Task<BulkPreviewResult> PreviewAsync(BulkPriceChangeRequest request, CancellationToken cancellationToken)
    {
        companyGuard.Require();
        var plan = await PlanAsync(request, cancellationToken);

        var pageIndex = Math.Max(request.PageIndex, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, MaxPreviewPageSize);
        var page = plan.Rows.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
        var references = await items.ReferencesAsync([.. page.Select(r => r.ItemId)], cancellationToken);

        var rows = page
            .Select(r => new BulkPreviewRow(
                r.ItemId,
                references.GetValueOrDefault(r.ItemId)?.Code ?? string.Empty,
                references.GetValueOrDefault(r.ItemId)?.Name ?? string.Empty,
                r.Version?.Id, r.Old, r.New, r.Skipped))
            .ToList();
        return new BulkPreviewResult(rows, plan.Rows.Count, pageIndex, pageSize, plan.Totals);
    }

    public async Task<BulkTotals> ApplyAsync(BulkPriceChangeRequest request, CancellationToken cancellationToken)
    {
        companyGuard.Require();
        var plan = await PlanAsync(request, cancellationToken);

        foreach (var row in plan.Rows.Where(r => r.Version is not null && r.New is not null))
        {
            var version = row.Version!;
            version.Edit(new PriceDetails(version.VersionName, version.EffectiveFrom, row.New!.MrpAmount, row.New.SaleAmount, row.New.PurchaseAmount));
        }

        return plan.Totals;
    }

    /// <summary>The one computation: for every selected item the named version, its old prices, the new ones or the reason it is skipped.</summary>
    private async Task<Plan> PlanAsync(BulkPriceChangeRequest request, CancellationToken cancellationToken)
    {
        var changes = (Mrp: request.Mrp?.ToChange(), Sale: request.Sale?.ToChange(), Purchase: request.Purchase?.ToChange());
        if (changes is { Mrp: null, Sale: null, Purchase: null })
            throw new ValidationFailureException(PriceMessages.InvalidChange);

        var itemIds = await items.AllIdsAsync((request.Selection ?? new PriceSelection()).ToItemSelection(), cancellationToken);
        if (itemIds.Count == 0)
            throw new ValidationFailureException(PriceMessages.NothingSelected);

        var versions = (await store.FindNamedForManagementAsync(itemIds, PriceVersion.NormalizeName(request.VersionName), cancellationToken))
            .ToDictionary(v => v.ItemId);
        var rows = itemIds.Select(id => PlanRow(id, versions.GetValueOrDefault(id), changes)).ToList();

        var totals = new BulkTotals(
            itemIds.Count,
            rows.Count(r => r.New is not null),
            rows.Count(r => r.Skipped == BulkSkipReason.NoVersion),
            rows.Count(r => r.Skipped == BulkSkipReason.BelowZero),
            rows.Count(r => r.Skipped == BulkSkipReason.Overflow));
        return new Plan(rows, totals);
    }

    private static PlannedRow PlanRow(Guid itemId, PriceVersion? version, (PriceChange? Mrp, PriceChange? Sale, PriceChange? Purchase) changes)
    {
        if (version is null)
            return new PlannedRow(itemId, null, null, null, BulkSkipReason.NoVersion);

        var old = new PriceTriple(version.MrpAmount, version.SaleAmount, version.PurchaseAmount);
        var mrp = Change(old.MrpAmount, changes.Mrp);
        var sale = Change(old.SaleAmount, changes.Sale);
        var purchase = Change(old.PurchaseAmount, changes.Purchase);

        var skipped = new[] { mrp, sale, purchase }.Select(o => o.Skipped).FirstOrDefault(s => s is not null);
        return skipped is { } reason
            ? new PlannedRow(itemId, version, old, null, reason == PriceChangeSkip.BelowZero ? BulkSkipReason.BelowZero : BulkSkipReason.Overflow)
            : new PlannedRow(itemId, version, old, new PriceTriple(mrp.NewValue!.Value, sale.NewValue!.Value, purchase.NewValue!.Value), null);
    }

    private static PriceChangeOutcome Change(decimal current, PriceChange? change) =>
        change is null ? new PriceChangeOutcome(current, null) : change.Apply(current);

    private sealed record PlannedRow(Guid ItemId, PriceVersion? Version, PriceTriple? Old, PriceTriple? New, BulkSkipReason? Skipped);

    private sealed record Plan(IReadOnlyList<PlannedRow> Rows, BulkTotals Totals);
}
