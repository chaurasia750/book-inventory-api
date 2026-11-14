using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using BookInventory.Application.Iam.Accounts;
using BookInventory.Application.Iam.Authentication;
using BookInventory.Domain.Iam;

namespace BookInventory.Application.Iam.Registration;

/// <summary>
/// R1/R16/R53/R58: checks the password policy and the uniqueness of the three identity values - which also holds while a
/// rejected application still reserves its e-mail and mobile - and adds the <c>MEMBER</c> account, pending or active.
/// Every other rule (the character sets, the length, "mobile required") is the aggregate's.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class MemberAccountCreator(
    IAccountStore store,
    UserIdentityService identity,
    IPasswordHasher hasher,
    AuthSettings settings,
    TimeProvider timeProvider)
{
    public async Task<AppUser> CreateAsync(MemberRegistration registration, bool activeImmediately, CancellationToken cancellationToken)
    {
        var failures = settings.Password.Validate(registration.Password);
        if (failures.Count > 0)
            throw new ValidationFailureException(failures[0]);

        await identity.EnsureAvailableAsync(
            new UserIdentity(registration.UserId, registration.Email, registration.Mobile), cancellationToken);

        var user = AppUser.RegisterMember(
            new UserDraft(registration.FullName, registration.UserId, registration.Email, registration.Mobile, hasher.Hash(registration.Password)),
            activeImmediately,
            actorId: Guid.Empty,
            timeProvider.GetUtcNow().UtcDateTime);

        await store.AddAsync(user, cancellationToken);
        return user;
    }
}
