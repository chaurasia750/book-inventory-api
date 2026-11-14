using BookInventory.BuildingBlocks.Application.Attributes;
using Microsoft.Extensions.DependencyInjection;
using BookInventory.Domain.Iam;

namespace BookInventory.Application.Iam.Authentication;

/// <summary>
/// Writes one <c>user_login_log</c> row per attempt (R20) and closes the row when its session ends (R24).
/// <para>
/// The row is the reason a failed login survives the 401: the use case returns a refusal instead of throwing, the
/// request transaction is marked <c>[PersistOnFailure]</c> and is committed anyway, so the attempt and the lockout
/// counter stay behind (R21). The password is never part of a row, and an identifier that matched no account is
/// masked by the aggregate itself.
/// </para>
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class LoginLogService(
    IAuthenticationStore store,
    IClientInfoParser parser,
    TimeProvider timeProvider)
{
    /// <summary>R20/R22: a successful login, linked to the session it just opened.</summary>
    public async Task RecordSuccessAsync(
        AppUser user,
        UserSession session,
        LoginContext context,
        CancellationToken cancellationToken)
    {
        var previous = await store.FindPreviousClientAsync(user.Id, cancellationToken);
        var loginLog = UserLoginLog.Success(user.Id, session.Id, BuildAttempt(context, previous));

        loginLog.SetClientDetails(parser.Parse(context.Client.UserAgent));
        await store.AddAsync(loginLog, cancellationToken);
    }

    /// <summary>
    /// R20/R21: a failed attempt. <paramref name="user"/> is <c>null</c> for an unknown identifier, in which case the
    /// row carries the masked identifier and nothing about any account (R8).
    /// </summary>
    public async Task RecordFailureAsync(
        AppUser? user,
        LoginResult result,
        LoginContext context,
        CancellationToken cancellationToken)
    {
        var attempt = BuildAttempt(context, previous: null);
        var loginLog = UserLoginLog.Failure(user?.Id, result, attempt);

        loginLog.SetClientDetails(parser.Parse(context.Client.UserAgent));
        await store.AddAsync(loginLog, cancellationToken);
    }

    /// <summary>
    /// R24: closes the row of a session that ended. Idempotent in the sense that a session whose row is already
    /// closed is left alone - two paths ending the same session must not throw (the aggregate owns that rule).
    /// </summary>
    public async Task CloseAsync(Guid sessionId, LogoutReason reason, DateTime at, CancellationToken cancellationToken)
    {
        var open = await store.FindOpenLogBySessionAsync(sessionId, cancellationToken);
        if (open is not null)
            open.Close(reason, at);
    }

    private LoginAttempt BuildAttempt(LoginContext context, PreviousClient? previous)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var agent = context.Client.UserAgent;
        var ip = context.Client.IpAddress;

        return new LoginAttempt(
            context.Identifier,
            now,
            new ClientInfo(ip, agent),
            context.Client.AcceptLanguage,
            IsNew(agent, previous?.UserAgent),
            IsNew(ip, previous?.IpAddress),
            context.Client.TraceId);
    }

    /// <summary>R22: a first-ever login counts as new device and new IP; after that only a different value does.</summary>
    private static bool IsNew(string? value, string? previous) =>
        previous is null || !string.Equals(value, previous, StringComparison.OrdinalIgnoreCase);
}
