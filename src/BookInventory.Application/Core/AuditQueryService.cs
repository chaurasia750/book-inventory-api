using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.BuildingBlocks.Domain.Models;
using Microsoft.Extensions.DependencyInjection;
using BookInventory.Application.Iam.Companies;
using BookInventory.Domain.Core;

namespace BookInventory.Application.Core;

/// <summary>
/// A7/A8: reads the audit trail for the caller. A Super Admin reads every entry and may filter by company; an Owner reads
/// only the entries of their own company, which comes from the session, so a <c>companyId</c> they send is replaced.
/// Who may call at all (Super Admin and Owner, never Staff) is decided at the endpoint.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class AuditQueryService(IAuditStore store, ICurrentUser currentUser, ICurrentCompany currentCompany)
{
    private const string SuperAdminRole = "SUPER_ADMIN";

    public Task<IPagedList<AuditEntryResponse>> SearchAsync(AuditSearchQuery query, CancellationToken cancellationToken) =>
        store.SearchAsync(IsSuperAdmin ? query : query with { CompanyId = RequireOwnCompany() }, cancellationToken);

    public Task<IReadOnlyList<string>> EntityTypesAsync(CancellationToken cancellationToken) =>
        store.EntityTypesAsync(IsSuperAdmin ? null : RequireOwnCompany(), cancellationToken);

    private bool IsSuperAdmin => currentUser.Role == SuperAdminRole;

    private Guid RequireOwnCompany() =>
        currentCompany.CompanyId ?? throw new ForbiddenException(CoreMessages.AuditCompanyRequired);
}
