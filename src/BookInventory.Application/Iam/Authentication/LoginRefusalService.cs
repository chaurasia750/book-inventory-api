using BookInventory.BuildingBlocks.Application.Attributes;
using Microsoft.Extensions.DependencyInjection;
using BookInventory.Application.Iam.Accounts;
using BookInventory.Domain.Iam;

namespace BookInventory.Application.Iam.Authentication;

/// <summary>
/// Every way a login attempt can be refused, split from <see cref="LoginService"/> for two reasons: all of them are
/// <em>returned</em> and never thrown, because the failed-login counter, the timeline row and the login-log row must be
/// committed although the request answers 401/423 (R21); and the lift of an expired suspension has to write its timeline
/// row (R33), which would be a seventh dependency on the login use case.
/// <para>
/// The order of the checks in the caller is what keeps R8 true: the lockout and the password are checked <em>before</em>
/// the status, so an unknown identifier and a wrong password answer the identical generic error and a specific status
/// message can only follow a correct password.
/// </para>
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class LoginRefusalService(
    LoginLogService loginLog,
    UserStatusHistoryWriter history,
    IPasswordHasher passwordHasher,
    AuthSettings settings)
{
    /// <summary>R7/R8: the identifier was typed wrong, or belongs to no visible account.</summary>
    private sealed record Refusal(LoginResult Result, string Code, bool Locked);

    /// <summary>
    /// R8: no account enumeration. The same generic error and the same timing as a wrong password, so an attacker
    /// cannot tell an unknown identifier from a wrong one.
    /// </summary>
    public async Task<AuthOutcome<IssuedSession<LoginResponse>>> UnknownIdentifierAsync(
        LoginContext context,
        string password,
        CancellationToken cancellationToken)
    {
        passwordHasher.SpendVerificationTime(password);
        return await RefuseAsync(null, new Refusal(LoginResult.FailedUnknownUser, IamMessages.UserInvalidCredentials, Locked: false), context, cancellationToken);
    }

    /// <summary>
    /// R9: the account is locked. The attempt that trips the lock is answered by
    /// <see cref="BadPasswordAsync"/>; only the next one meets the 423 (AC3).
    /// </summary>
    public async Task<AuthOutcome<IssuedSession<LoginResponse>>> LockedAsync(
        AppUser user,
        LoginContext context,
        CancellationToken cancellationToken)
    {
        return await RefuseAsync(user, new Refusal(LoginResult.FailedLocked, IamMessages.UserAccountLocked, Locked: true), context, cancellationToken);
    }

    /// <summary>
    /// R9/R16: one more wrong password, and the row. The attempt that trips the lock still answers the generic error -
    /// a 423 here would tell an attacker that the account exists.
    /// </summary>
    public async Task<AuthOutcome<IssuedSession<LoginResponse>>> BadPasswordAsync(
        AppUser user,
        LoginContext context,
        DateTime now,
        CancellationToken cancellationToken)
    {
        user.RecordFailedLogin(now, settings.Lockout);
        return await RefuseAsync(user, new Refusal(LoginResult.FailedBadPassword, IamMessages.UserInvalidCredentials, Locked: false), context, cancellationToken);
    }

    /// <summary>
    /// R32/R3/R8: lifts a due suspension first - and records that lift with actor <c>SYSTEM</c>, so the timeline of R33
    /// is the same whether the lift came from here or from the periodic job - then answers the specific status message,
    /// without the internal reason, only now that the password is known to be right. <c>null</c> means the account may
    /// log in.
    /// </summary>
    public async Task<AuthFailure?> StatusAsync(
        AppUser user,
        LoginContext context,
        DateTime now,
        CancellationToken cancellationToken)
    {
        await LiftDueSuspensionAsync(user, now, cancellationToken);

        if (user.StatusRefusal is not { } result)
            return null;

        await loginLog.RecordFailureAsync(user, result, context, cancellationToken);
        return AuthFailure.Unauthenticated(MessageFor(result));
    }

    /// <summary>R20: the one row of this attempt, plus the 401/423 the caller answers.</summary>
    private async Task<AuthOutcome<IssuedSession<LoginResponse>>> RefuseAsync(
        AppUser? user,
        Refusal refusal,
        LoginContext context,
        CancellationToken cancellationToken)
    {
        await loginLog.RecordFailureAsync(user, refusal.Result, context, cancellationToken);
        return refusal.Locked
            ? AuthOutcome<IssuedSession<LoginResponse>>.Denied(AuthFailure.Locked(refusal.Code))
            : AuthOutcome<IssuedSession<LoginResponse>>.Denied(refusal.Code);
    }

    /// <summary>
    /// The system account is never suspended (R45), and the timeline is about a user, so this guard belongs here and
    /// not in the shared history writer.
    /// </summary>
    private async Task LiftDueSuspensionAsync(AppUser user, DateTime now, CancellationToken cancellationToken)
    {
        if (user.IsSystemAccount || !user.LiftSuspensionIfDue(now))
            return;

        await history.RecordAsync(
            user,
            new StatusTransition(AccountStatus.Suspended, user.Status, user.StatusReason),
            cancellationToken);
    }

    /// <summary>R8: the specific message a status refusal shows, only ever after the password proved right.</summary>
    private static string MessageFor(LoginResult result) => result switch
    {
        LoginResult.FailedPending => IamMessages.UserPendingApproval,
        LoginResult.FailedInactive => IamMessages.UserAccountInactive,
        LoginResult.FailedSuspended => IamMessages.UserAccountSuspended,
        _ => IamMessages.UserApplicationRejected
    };
}
