using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.Domain.Customer;
using Microsoft.Extensions.DependencyInjection;
using CustomerRoot = BookInventory.Domain.Customer.Customer;

namespace BookInventory.Application.Customer;

/// <summary>
/// R2-R7, R5: edits a customer and switches it off and on. Every rule is the aggregate's; this class loads, calls one behaviour
/// and lets the request transaction commit. A customer of another company is not found (P19).
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class CustomerLifecycleService(ICustomerStore store, CustomerSchoolGuard schoolGuard)
{
    public async Task EditAsync(Guid id, EditCustomerRequest request, CancellationToken cancellationToken)
    {
        var customer = await LoadAsync(id, cancellationToken);
        var className = await schoolGuard.ResolveClassAsync(request.SchoolId, request.ClassName, customer.SchoolId, cancellationToken);
        if (await store.IsMobileTakenAsync(CustomerRoot.NormalizeMobile(request.Mobile), customer.Id, cancellationToken))
            throw new ConflictException(CustomerMessages.MobileTaken);

        customer.Edit(request.ToDetails(className));
    }

    public async Task DeactivateAsync(Guid id, CancellationToken cancellationToken) =>
        (await LoadAsync(id, cancellationToken)).Deactivate();

    public async Task ReactivateAsync(Guid id, CancellationToken cancellationToken) =>
        (await LoadAsync(id, cancellationToken)).Reactivate();

    private async Task<CustomerRoot> LoadAsync(Guid id, CancellationToken cancellationToken) =>
        await store.FindForManagementAsync(id, cancellationToken) ?? throw new NotFoundException(CustomerMessages.NotFound);
}
