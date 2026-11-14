using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using BookInventory.Application.Iam.Passwords;
using BookInventory.Domain.Iam;

namespace BookInventory.Application.Iam.Accounts;

/// <summary>
/// The account data a Super Admin writes: a new staff account (R2), somebody else's profile (R35) and the release of a
/// rejected agent's e-mail and mobile (R58). Every rule - the character set, the length, the staff role, "not your own
/// account", "only a rejected account can be released" - is the aggregate's; this class loads, calls one behaviour and
/// lets the request transaction commit.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class UserAccountService(
    IAccountStore store,
    UserIdentityService identity,
    IPasswordHasher hasher,
    TemporaryPasswordGenerator temporaryPasswords,
    ICurrentUser currentUser,
    TimeProvider timeProvider)
{
    /// <summary>
    /// R2/R57: a staff account exists only because a Super Admin made it, it is active at once and its first login must
    /// change the password. The temporary password is generated here, hashed like any other and returned in the answer
    /// <em>once</em>; nothing can fetch it again, so a Super Admin who loses it sends a reset link instead (R36).
    /// </summary>
    public async Task<CreatedStaffResponse> CreateStaffAsync(CreateStaffRequest request, CancellationToken cancellationToken)
    {
        var role = request.Role ?? throw new ValidationFailureException(IamMessages.UserRoleRequired);
        await identity.EnsureAvailableAsync(
            new UserIdentity(request.UserName, request.Email, request.Mobile), cancellationToken);

        var temporaryPassword = temporaryPasswords.Generate();
        var user = AppUser.CreateStaff(
            new UserDraft(request.FullName, request.UserName, request.Email, request.Mobile, hasher.Hash(temporaryPassword)),
            role,
            currentUser.UserId,
            Now());

        await store.AddAsync(user, cancellationToken);

        return new CreatedStaffResponse(
            user.Id, user.UserName, user.FullName, user.Email, user.Mobile,
            user.Role, user.Status, user.MustChangePassword, temporaryPassword);
    }

    /// <summary>
    /// R35/AC15: full name, e-mail and mobile of somebody else's account, with the uniqueness of R1 checked first so a
    /// duplicate is a 409 with the name of the offending field. Your own account is refused here - that goes through
    /// the self-service endpoint, which asks for the current password.
    /// </summary>
    public async Task UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var user = await LoadAsync(id, cancellationToken);
        await identity.EnsureAvailableAsync(
            new UserIdentity(UserName: null, Email: request.Email, Mobile: request.Mobile), cancellationToken, user.Id);

        user.UpdateProfile(request.FullName, request.Email, request.Mobile, currentUser.UserId);
    }

    /// <summary>
    /// R58/AC27: a rejected agent keeps their e-mail and mobile reserved, so nobody else can apply with them, until a
    /// Super Admin releases them. The account itself stays as history (R37) - only the two values are freed.
    /// </summary>
    public async Task ReleaseIdentityAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await LoadAsync(id, cancellationToken);
        user.ReleaseIdentity(currentUser.UserId, Now());
    }

    /// <summary>404 when no such id at all; the hidden system account is found on purpose (see <see cref="IAccountStore"/>).</summary>
    private async Task<AppUser> LoadAsync(Guid id, CancellationToken cancellationToken) =>
        await store.FindForManagementAsync(id, cancellationToken)
        ?? throw new NotFoundException(IamMessages.UserNotFound);

    private DateTime Now() => timeProvider.GetUtcNow().UtcDateTime;
}
