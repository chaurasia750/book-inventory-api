using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using BookInventory.Application.Iam.Companies;
using BookInventory.Domain.Company;
using CompanyRoot = BookInventory.Domain.Company.Company;

namespace BookInventory.Application.Company;

/// <summary>
/// R2/R3/R7/R8: edits a company and switches it off and on. Every rule is the aggregate's; this class loads, calls one
/// behaviour and lets the request transaction commit. Deactivation also ends every session of the company's accounts,
/// through a plain call to <c>iam</c> (a domain event handler would run after the save).
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class CompanyLifecycleService(
    ICompanyStore store,
    ICompanySessionRevoker sessions,
    ICurrentUser currentUser,
    TimeProvider timeProvider)
{
    public async Task EditAsync(Guid id, EditCompanyRequest request, CancellationToken cancellationToken)
    {
        var company = await LoadAsync(id, cancellationToken);
        company.Edit(request.ToDetails());

        if (await store.IsNameTakenAsync(company.Name, company.Id, cancellationToken))
            throw new ConflictException(CompanyMessages.NameTaken);
    }

    public async Task DeactivateAsync(Guid id, DeactivateCompanyRequest request, CancellationToken cancellationToken)
    {
        var company = await LoadAsync(id, cancellationToken);
        company.Deactivate(request.Reason, currentUser.UserId, Now());
        await sessions.RevokeAllAsync(company.Id, cancellationToken);
    }

    public async Task ReactivateAsync(Guid id, ActivateCompanyRequest request, CancellationToken cancellationToken)
    {
        var company = await LoadAsync(id, cancellationToken);
        company.Reactivate(request.Note, currentUser.UserId, Now());
    }

    private async Task<CompanyRoot> LoadAsync(Guid id, CancellationToken cancellationToken) =>
        await store.FindForManagementAsync(id, cancellationToken) ?? throw new NotFoundException(CompanyMessages.NotFound);

    private DateTime Now() => timeProvider.GetUtcNow().UtcDateTime;
}
