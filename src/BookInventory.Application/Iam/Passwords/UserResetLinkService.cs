using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using BookInventory.Domain.Iam;

namespace BookInventory.Application.Iam.Passwords;

/// <summary>
/// R36: a Super Admin sends a password reset link to a user who lost theirs. It is the R17 flow of
/// <see cref="ForgotPasswordService"/> without the rate limit and without the 202-behind-everything: this is an
/// administrative action, so a missing user, a system account or an account that may not log in is a real error the
/// Super Admin has to see.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class UserResetLinkService(IUserDirectory directory, PasswordResetLinkIssuer links)
{
    /// <summary>
    /// The link always goes to the address already stored for the account, never to one supplied here, and only an
    /// <c>ACTIVE</c> account can use it - a link for a suspended or pending account would arrive as a 400 at the reset
    /// screen instead of a clear message now.
    /// </summary>
    public async Task SendAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await directory.GetVisibleByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(IamMessages.UserNotFound);

        if (user.Status != AccountStatus.Active)
            throw new ConflictException(IamMessages.UserResetLinkNotAllowed);

        if (user.Email is not { } email)
            throw new ConflictException(IamMessages.UserEmailMissing);

        await links.IssueAsync(new ResetLinkRecipient(user.Id, user.FullName, email), cancellationToken);
    }
}
