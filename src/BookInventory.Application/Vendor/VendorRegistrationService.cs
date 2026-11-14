using BookInventory.Application.Core;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.Domain.Vendor;
using Microsoft.Extensions.DependencyInjection;
using VendorRoot = BookInventory.Domain.Vendor.Vendor;

namespace BookInventory.Application.Vendor;

/// <summary>
/// R1-R7, R11: creates a vendor in the company of the session. The code comes from the company code series (platform P23)
/// under its lock, so parallel creations get different numbers; the company itself is stamped by the platform when the row is
/// saved. Any refused value throws and the request transaction saves nothing, the number included.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class VendorRegistrationService(IVendorStore store, ICompanyCodes codes)
{
    public async Task<VendorDetailResponse> CreateAsync(CreateVendorRequest request, CancellationToken cancellationToken)
    {
        if (await store.IsNameTakenAsync(VendorRoot.NormalizeName(request.Name), excludingId: null, cancellationToken))
            throw new ConflictException(VendorMessages.NameTaken);

        var vendor = VendorRoot.Create(await codes.NextAsync(VendorRoot.CodePrefix, cancellationToken), request.ToDetails());
        await store.AddAsync(vendor, cancellationToken);
        return VendorResponses.Detail(vendor);
    }
}
