using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.BuildingBlocks.Domain.Models;
using Microsoft.Extensions.DependencyInjection;
using BookInventory.Application.Iam.Accounts;
using BookInventory.Application.Iam.Companies;
using BookInventory.Domain.Iam;

namespace BookInventory.Application.Iam.Staff;

/// <summary>
/// R66: the Owner's reads of their own company's Staff. The list is forced to the Owner's company and the Staff role
/// whatever the query string says; a detail or a session list is asked for only after <see cref="OwnStaffGuard"/> agreed.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class StaffQueryService(OwnStaffGuard guard, UserQueryService users, ICurrentCompany currentCompany)
{
    public Task<IPagedList<UserSummary>> SearchAsync(UserSearchQuery query, CancellationToken cancellationToken)
    {
        var companyId = currentCompany.CompanyId ?? throw new ForbiddenException(IamMessages.UserCompanyInvalid);
        return users.SearchAsync(query with { CompanyId = companyId, Role = Role.Staff }, cancellationToken);
    }

    public async Task<UserDetail> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        await guard.RequireAsync(id, cancellationToken);
        return await users.GetAsync(id, cancellationToken);
    }

    public async Task<UserSessionItem[]> GetSessionsAsync(Guid id, CancellationToken cancellationToken)
    {
        await guard.RequireAsync(id, cancellationToken);
        return await users.GetSessionsAsync(id, cancellationToken);
    }
}
