using BookInventory.Application.Core;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.Domain.Item;
using Microsoft.Extensions.DependencyInjection;
using ItemRoot = BookInventory.Domain.Item.Item;

namespace BookInventory.Application.Item;

/// <summary>
/// R1-R8, R10: creates an item in the company of the session. The code comes from the company code series (platform P23) under
/// its lock; the vendor and the barcodes are checked before anything is saved. Any refused value throws and the request
/// transaction saves nothing, the number included.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class ItemRegistrationService(IItemStore store, ICompanyCodes codes, ItemVendorGuard vendorGuard)
{
    public async Task<ItemDetailResponse> CreateAsync(CreateItemRequest request, CancellationToken cancellationToken)
    {
        await vendorGuard.RequireUsableAsync(request.VendorId, currentVendorId: null, cancellationToken);

        var item = ItemRoot.Create(await codes.NextAsync(ItemRoot.CodePrefix, cancellationToken), request.ToDetails());
        if (request.Barcodes is { Count: > 0 } barcodes)
        {
            item.ReplaceBarcodes(barcodes);
            await RequireFreeAsync(item, excludingItemId: null, cancellationToken);
        }

        await store.AddAsync(item, cancellationToken);
        return ItemResponses.Detail(item, vendorName: null);
    }

    private async Task RequireFreeAsync(ItemRoot item, Guid? excludingItemId, CancellationToken cancellationToken)
    {
        var barcodes = item.Barcodes.Select(b => b.Barcode).ToList();
        if (await store.AnyBarcodeUsedAsync(barcodes, excludingItemId, cancellationToken))
            throw new ConflictException(ItemMessages.BarcodeTaken);
    }
}
