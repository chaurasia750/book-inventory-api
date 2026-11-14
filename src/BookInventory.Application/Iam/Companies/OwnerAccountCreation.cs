using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain;
using Microsoft.Extensions.DependencyInjection;
using BookInventory.Application.Iam.Accounts;
using BookInventory.Application.Iam.Passwords;
using BookInventory.Domain.Iam;

namespace BookInventory.Application.Iam.Companies;

/// <summary>
/// R1/R57/R61: checks that the user id (and the e-mail and mobile, when given) are free, generates the temporary
/// password, and adds the <c>OWNER</c> account. Every other rule - the character set of the user id, the length of the
/// name, "the company id is a real id" - is the aggregate's.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped, typeof(IOwnerAccountCreation))]
public sealed class OwnerAccountCreation(
    IAccountStore store,
    UserIdentityService identity,
    IPasswordHasher hasher,
    TemporaryPasswordGenerator temporaryPasswords,
    ICurrentUser currentUser,
    TimeProvider timeProvider) : IOwnerAccountCreation
{
    public async Task<OwnerAccountCreated> CreateAsync(OwnerRegistration registration, CancellationToken cancellationToken = default)
    {
        await identity.EnsureAvailableAsync(
            new UserIdentity(registration.UserId, registration.Email, registration.Mobile), cancellationToken);

        var temporaryPassword = temporaryPasswords.Generate();
        var owner = AppUser.CreateOwner(
            new UserDraft(registration.FullName, registration.UserId, registration.Email, registration.Mobile, hasher.Hash(temporaryPassword)),
            registration.CompanyId,
            currentUser.UserId,
            timeProvider.GetUtcNow().UtcDateTime);

        await store.AddAsync(owner, cancellationToken);
        return new OwnerAccountCreated(owner.Id, owner.UserName, temporaryPassword);
    }
}
