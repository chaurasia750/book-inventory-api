using BookInventory.BuildingBlocks.Application.Attributes;
using Microsoft.Extensions.DependencyInjection;
using BookInventory.BuildingBlocks.Domain;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.Domain.Iam;

namespace BookInventory.Application.Iam.Authentication;

/// <summary>
/// R12/R27: <c>GET auth/me</c> - the caller's own profile, role and forced-change flag. The id comes from the validated
/// token and the lookup runs through the authentication store, so this can never return another user and, unlike the
/// user list, still answers for the hidden system account (R44: it authorizes like a Super Admin, so the UI needs its
/// profile too).
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class MeService(IAuthenticationStore store, ICurrentUser currentUser)
{
    public async Task<LoggedInUser> GetMeAsync(CancellationToken cancellationToken)
    {
        var user = await store.FindUserByIdAsync(currentUser.UserId, cancellationToken)
                   ?? throw new NotFoundException(IamMessages.UserNotFound);

        return LoggedInUser.From(user);
    }
}
