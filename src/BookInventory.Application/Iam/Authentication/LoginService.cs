using BookInventory.BuildingBlocks.Application.Attributes;
using Microsoft.Extensions.DependencyInjection;
using BookInventory.Domain.Iam;

namespace BookInventory.Application.Iam.Authentication;

/// <summary>
/// R7-R12, R20-R22, R32: the login use case. It loads the account, runs the checks in the order that keeps R8 true and
/// then either refuses or grants - the two branches live in <see cref="LoginRefusalService"/> and
/// <see cref="LoginGrantService"/>, so this class is the flow of one attempt and nothing else.
/// <para>
/// Order of the checks: the lockout and the password come <em>before</em> the status, so an unknown identifier and a
/// wrong password answer the identical generic error and a specific status message can only follow a correct password.
/// </para>
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class LoginService(
    IAuthenticationStore store,
    IPasswordHasher passwordHasher,
    LoginRefusalService refusals,
    LoginGrantService grant,
    SystemAccountSettings systemAccount,
    TimeProvider timeProvider)
{
    public async Task<AuthOutcome<IssuedSession<LoginResponse>>> LoginAsync(
        LoginCommand command,
        CancellationToken cancellationToken)
    {
        var context = new LoginContext(
            command.Login.Identifier?.Trim() ?? string.Empty,
            command.Client,
            command.Login.RememberMe);
        var password = command.Login.Password ?? string.Empty;
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var user = await store.FindUserForLoginAsync(context.Identifier, cancellationToken);

        // R46/R47: a disabled account, or a login from an address outside the allow-list, is an unknown user.
        if (user is { IsSystemAccount: true } && !systemAccount.Admits(context.Client.IpAddress))
            user = null;

        if (user is null)
            return await refusals.UnknownIdentifierAsync(context, password, cancellationToken);

        if (user.IsLockedAt(now))
            return await refusals.LockedAsync(user, context, cancellationToken);

        if (!PasswordMatches(user, password))
            return await refusals.BadPasswordAsync(user, context, now, cancellationToken);

        var refused = await refusals.StatusAsync(user, context, now, cancellationToken);
        return refused is not null
            ? AuthOutcome<IssuedSession<LoginResponse>>.Denied(refused)
            : await grant.GrantAsync(user, context, now, cancellationToken);
    }

    /// <summary>
    /// R16: verifies the password and, when it was hashed with weaker parameters, upgrades the stored hash in place -
    /// the same login, no forced change, no session revoked.
    /// </summary>
    private bool PasswordMatches(AppUser user, string password)
    {
        var verification = passwordHasher.Verify(password, user.PasswordHash);
        if (verification == PasswordVerificationResult.Failed)
            return false;

        if (verification == PasswordVerificationResult.SuccessRehashNeeded)
            user.UpgradePasswordHash(passwordHasher.Hash(password));

        return true;
    }
}
