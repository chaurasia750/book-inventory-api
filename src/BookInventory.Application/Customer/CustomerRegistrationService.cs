using BookInventory.Application.Core;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.Domain.Customer;
using Microsoft.Extensions.DependencyInjection;
using CustomerRoot = BookInventory.Domain.Customer.Customer;

namespace BookInventory.Application.Customer;

/// <summary>
/// R1-R7, R10: creates a customer in the company of the session. The code comes from the company code series (platform P23)
/// under its lock, so parallel creations get different numbers; the company itself is stamped by the platform when the row is
/// saved. Any refused value throws and the request transaction saves nothing, the number included.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class CustomerRegistrationService(
    ICustomerStore store, ICompanyCodes codes, CustomerSchoolGuard schoolGuard, CustomerQueryService queries)
{
    public async Task<CustomerDetailResponse> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken)
    {
        var className = await schoolGuard.ResolveClassAsync(request.SchoolId, request.ClassName, null, cancellationToken);
        var details = request.ToDetails(className);
        if (await store.IsMobileTakenAsync(CustomerRoot.NormalizeMobile(request.Mobile), null, cancellationToken))
            throw new ConflictException(CustomerMessages.MobileTaken);

        var customer = CustomerRoot.Create(await codes.NextAsync(CustomerRoot.CodePrefix, cancellationToken), details);
        await store.AddAsync(customer, cancellationToken);
        return await queries.DetailOfAsync(customer, cancellationToken);
    }
}
