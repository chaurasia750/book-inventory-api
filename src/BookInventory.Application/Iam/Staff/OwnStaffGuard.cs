using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using BookInventory.Application.Iam.Accounts;
using BookInventory.Application.Iam.Companies;
using BookInventory.Domain.Iam;

namespace BookInventory.Application.Iam.Staff;

/// <summary>
/// R66: the one place that decides "this is a Staff account of the caller's own company". Anything else - another
/// company's account, an Owner, a Super Admin, an unknown id - answers the same 404, so an Owner cannot probe other
/// accounts by trying ids.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class OwnStaffGuard(IAccountStore store, ICurrentCompany currentCompany)
{
    public async Task<AppUser> RequireAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await store.FindForManagementAsync(id, cancellationToken);

        return user is { Role: Role.Staff } && user.CompanyId is { } companyId && companyId == currentCompany.CompanyId
            ? user
            : throw new NotFoundException(IamMessages.UserNotFound);
    }
}
