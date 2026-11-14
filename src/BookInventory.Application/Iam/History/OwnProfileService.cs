using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using BookInventory.Application.Iam.Accounts;
using BookInventory.Application.Iam.Authentication;
using BookInventory.Domain.Iam;

namespace BookInventory.Application.Iam.History;

/// <summary>
/// R35, R41: a user edits their own profile. The full name is free; an e-mail or a mobile that would <em>change</em> needs the
/// current password, because a stolen access token must not be enough to take over the account's recovery address. The
/// uniqueness of R1 is the same lookup the Super Admin's edit uses (<see cref="UserIdentityService"/>), and the shape of
/// every value is the aggregate's.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class OwnProfileService(
    IAuthenticationStore store,
    UserIdentityService identity,
    IPasswordHasher hasher,
    ICurrentUser currentUser)
{
    public async Task UpdateAsync(UpdateOwnProfileRequest request, CancellationToken cancellationToken)
    {
        var user = await store.FindUserByIdAsync(currentUser.UserId, cancellationToken)
                   ?? throw new UnauthenticatedException(IamMessages.SessionNotFound);

        if (user.WouldChangeContact(request.Email, request.Mobile))
        {
            if (string.IsNullOrEmpty(request.CurrentPassword)
                || hasher.Verify(request.CurrentPassword, user.PasswordHash) == PasswordVerificationResult.Failed)
                throw new ValidationFailureException(IamMessages.PasswordCurrentIncorrect);

            await identity.EnsureAvailableAsync(
                new UserIdentity(UserName: null, request.Email, request.Mobile), cancellationToken, user.Id);
        }

        user.UpdateOwnProfile(request.FullName, request.Email, request.Mobile);
    }
}
