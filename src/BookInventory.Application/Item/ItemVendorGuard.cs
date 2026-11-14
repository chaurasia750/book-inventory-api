using BookInventory.Application.Vendor;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.Domain.Item;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Application.Item;

/// <summary>
/// R5: the vendor of an item must be an active vendor of the company, read through the vendor directory (vendor R15). A
/// vendor that an item already has may stay even when it was deactivated later; only a newly chosen one must be active.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class ItemVendorGuard(IVendorDirectory vendors)
{
    public async Task RequireUsableAsync(Guid? vendorId, Guid? currentVendorId, CancellationToken cancellationToken)
    {
        if (vendorId is null || vendorId == currentVendorId)
            return;

        var vendor = await vendors.GetAsync(vendorId.Value, cancellationToken);
        if (vendor is not { IsActive: true })
            throw new ValidationFailureException(ItemMessages.InvalidVendor);
    }
}
