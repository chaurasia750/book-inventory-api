using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using BookInventory.Application.Iam.Companies;
using BookInventory.Domain.Company;

namespace BookInventory.Application.Company;

/// <summary>
/// R13: a Super Admin enters an active company and leaves it again. The use case checks the company, asks <c>iam</c> to
/// store the choice on the caller's own session (which also issues the token that carries it) and records the action
/// with the caller's real identity (R12). A company that does not exist is 404, an inactive one 409.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class CompanyEntryService(
    ICompanyStore store,
    ISessionCompanySelector selector,
    ICurrentUser currentUser,
    TimeProvider timeProvider)
{
    public async Task<SessionCompanyResponse> EnterAsync(Guid companyId, CancellationToken cancellationToken)
    {
        var company = await store.FindForReadAsync(companyId, cancellationToken)
            ?? throw new NotFoundException(CompanyMessages.NotFound);
        if (!company.IsActive)
            throw new ConflictException(CompanyMessages.NotActive);

        var change = await selector.EnterAsync(companyId, cancellationToken);
        await store.AddAsync(CompanyEntry.Entered(companyId, currentUser.UserId, Now()), cancellationToken);

        return new SessionCompanyResponse(
            new CurrentCompanyResponse(company.Id, company.Name, company.Status, company.State),
            change.AccessToken.Token,
            change.AccessToken.ExpiresAt);
    }

    public async Task<SessionCompanyResponse> LeaveAsync(CancellationToken cancellationToken)
    {
        var change = await selector.LeaveAsync(cancellationToken);
        if (change.PreviousCompanyId is { } previous)
            await store.AddAsync(CompanyEntry.Left(previous, currentUser.UserId, Now()), cancellationToken);

        return new SessionCompanyResponse(null, change.AccessToken.Token, change.AccessToken.ExpiresAt);
    }

    private DateTime Now() => timeProvider.GetUtcNow().UtcDateTime;
}
