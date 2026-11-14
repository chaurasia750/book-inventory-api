using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.Domain.Item;
using Microsoft.Extensions.DependencyInjection;
using ItemRoot = BookInventory.Domain.Item.Item;

namespace BookInventory.Application.Item;

/// <summary>
/// R2-R8: edits an item, replaces its barcode list and switches it off and on. Every rule is the aggregate's; this class loads,
/// checks what needs another module or another item (the vendor, a free barcode), calls one behaviour and lets the request
/// transaction commit. An item of another company is not found (P19).
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class ItemLifecycleService(IItemStore store, ItemVendorGuard vendorGuard)
{
    public async Task EditAsync(Guid id, EditItemRequest request, CancellationToken cancellationToken)
    {
        var item = await LoadAsync(id, cancellationToken);
        await vendorGuard.RequireUsableAsync(request.VendorId, item.VendorId, cancellationToken);
        item.Edit(request.ToDetails(), request.IsStockManaged);
    }

    public async Task ReplaceBarcodesAsync(Guid id, ReplaceBarcodesRequest request, CancellationToken cancellationToken)
    {
        var item = await LoadAsync(id, cancellationToken);
        item.ReplaceBarcodes(request.Barcodes);

        var barcodes = item.Barcodes.Select(b => b.Barcode).ToList();
        if (await store.AnyBarcodeUsedAsync(barcodes, item.Id, cancellationToken))
            throw new ConflictException(ItemMessages.BarcodeTaken);
    }

    public async Task DeactivateAsync(Guid id, CancellationToken cancellationToken) =>
        (await LoadAsync(id, cancellationToken)).Deactivate();

    public async Task ReactivateAsync(Guid id, CancellationToken cancellationToken) =>
        (await LoadAsync(id, cancellationToken)).Reactivate();

    private async Task<ItemRoot> LoadAsync(Guid id, CancellationToken cancellationToken) =>
        await store.FindForManagementAsync(id, cancellationToken) ?? throw new NotFoundException(ItemMessages.NotFound);
}
