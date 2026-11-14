using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using BookInventory.Domain.Iam;

namespace BookInventory.Application.Iam.Accounts;

/// <summary>
/// R1/AC15: the uniqueness of user name, e-mail and mobile, checked before the account is written so the caller gets a
/// 409 with a code that says <em>which</em> value is taken. The database indexes are the real guarantee - two concurrent
/// requests can both pass a lookup - but a lookup turns the losing race into a readable answer instead of a provider
/// error.
/// <para>
/// The values are compared in their stored form, produced by <see cref="IdentityNormalizer"/> - the same normalisation
/// the aggregate applies - and the account being edited is excluded, so saving a form that leaves a value unchanged is
/// not a duplicate of itself.
/// </para>
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class UserIdentityService(IAccountStore store)
{
    /// <summary>Throws the 409 for the first of the three values that is already in use; the user name is checked first.</summary>
    public async Task EnsureAvailableAsync(UserIdentity identity, CancellationToken cancellationToken, Guid? excludingUserId = null)
    {
        var userName = IdentityNormalizer.NormalizeUserName(identity.UserName);
        var email = IdentityNormalizer.NormalizeEmail(identity.Email);
        var mobile = IdentityNormalizer.NormalizeMobile(identity.Mobile);

        if (userName is not null && await store.IsUserNameTakenAsync(userName, excludingUserId, cancellationToken))
            throw new ConflictException(IamMessages.UserDuplicateUserName);
        if (email is not null && await store.IsEmailTakenAsync(email, excludingUserId, cancellationToken))
            throw new ConflictException(IamMessages.UserDuplicateEmail);
        if (mobile is not null && await store.IsMobileTakenAsync(mobile, excludingUserId, cancellationToken))
            throw new ConflictException(IamMessages.UserDuplicateMobile);
    }
}
