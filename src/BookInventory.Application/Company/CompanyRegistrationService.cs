using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using BookInventory.Application.Iam.Companies;
using BookInventory.Domain.Company;
using CompanyRoot = BookInventory.Domain.Company.Company;

namespace BookInventory.Application.Company;

/// <summary>
/// R3/R4: creates a company together with its Owner in the one request transaction. The aggregate validates the name and
/// generates the Owner's user id; <c>iam</c> checks that id is free and makes the account. Whatever fails - a taken name,
/// a taken id, a short full name - throws, and the transaction saves nothing.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class CompanyRegistrationService(
    ICompanyStore store,
    IOwnerAccountCreation owners,
    ICurrentUser currentUser,
    TimeProvider timeProvider)
{
    public async Task<CreatedCompanyResponse> CreateAsync(CreateCompanyRequest request, CancellationToken cancellationToken)
    {
        var company = CompanyRoot.Create(request.ToDetails(), currentUser.UserId, timeProvider.GetUtcNow().UtcDateTime);
        if (await store.IsNameTakenAsync(company.Name, excludingId: null, cancellationToken))
            throw new ConflictException(CompanyMessages.NameTaken);

        var ownerUserId = company.OwnerUserId;
        await store.AddAsync(company, cancellationToken);
        var owner = await owners.CreateAsync(new OwnerRegistration(request.Owner.FullName, ownerUserId, company.Id), cancellationToken);

        return new CreatedCompanyResponse(
            company.Id, company.Name, company.Status,
            new CompanyOwnerCredentials(owner.UserId, owner.UserName, owner.TemporaryPassword));
    }
}
