using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.Domain.Vendor;
using Microsoft.Extensions.DependencyInjection;
using VendorRoot = BookInventory.Domain.Vendor.Vendor;

namespace BookInventory.Application.Vendor;

/// <summary>
/// R2-R7: edits a vendor and switches it off and on. Every rule is the aggregate's; this class loads, calls one behaviour and
/// lets the request transaction commit. A vendor of another company is not found (P19).
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class VendorLifecycleService(IVendorStore store)
{
    public async Task EditAsync(Guid id, EditVendorRequest request, CancellationToken cancellationToken)
    {
        var vendor = await LoadAsync(id, cancellationToken);
        if (await store.IsNameTakenAsync(VendorRoot.NormalizeName(request.Name), vendor.Id, cancellationToken))
            throw new ConflictException(VendorMessages.NameTaken);

        vendor.Edit(request.ToDetails());
    }

    public async Task DeactivateAsync(Guid id, CancellationToken cancellationToken) =>
        (await LoadAsync(id, cancellationToken)).Deactivate();

    public async Task ReactivateAsync(Guid id, CancellationToken cancellationToken) =>
        (await LoadAsync(id, cancellationToken)).Reactivate();

    private async Task<VendorRoot> LoadAsync(Guid id, CancellationToken cancellationToken) =>
        await store.FindForManagementAsync(id, cancellationToken) ?? throw new NotFoundException(VendorMessages.NotFound);
}
