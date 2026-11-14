using BookInventory.BuildingBlocks.Application.Attributes;
using Microsoft.Extensions.DependencyInjection;
using BookInventory.Domain.Iam;

namespace BookInventory.Application.Iam.Authentication;

/// <summary>
/// The one way a session ends (R14, R24, R31): it is revoked with a reason, the login-log row it belongs to is closed
/// with the same reason and time, and the cached "is this session alive?" answer is dropped so the access token stops
/// working on the very next request. Logout, refresh, expiry and the periodic sweep all come through here, so the three
/// steps can never drift apart. Ending a session that is already revoked only repairs the log row and the cache.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class SessionEndService(LoginLogService loginLog, ISessionGuard sessionGuard)
{
    public async Task EndAsync(UserSession session, LogoutReason reason, DateTime at, CancellationToken cancellationToken)
    {
        session.Revoke(reason, at);
        await loginLog.CloseAsync(session.Id, reason, at, cancellationToken);
        sessionGuard.Invalidate(session.Id);
    }
}
