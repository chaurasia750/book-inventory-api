using BookInventory.BuildingBlocks.Application.Attributes;
using Microsoft.Extensions.Caching.Memory;
using BookInventory.Application.Iam.Authentication;

namespace BookInventory.Infrastructure.Iam;

/// <summary>
/// The per-request "is this session still alive?" check of R31 groundwork, cached for
/// <see cref="CacheSeconds"/> seconds. The cache is only a window: every revoke calls <see cref="Invalidate"/>, so a
/// logout, a forced logout or an admin revocation takes effect on the very next request instead of after the window.
/// A single instance is assumed, which is what the deployment is (see the plan's risks).
/// </summary>
[AutoRegister(ServiceLifetime.Scoped, typeof(ISessionGuard))]
public sealed class SessionGuard(IMemoryCache cache, IAuthenticationStore store, TimeProvider timeProvider) : ISessionGuard
{
    private const int CacheSeconds = 30;

    public async Task<bool> IsActiveAsync(Guid sessionId, CancellationToken cancellationToken = default) =>
        await cache.GetOrCreateAsync(CacheKey(sessionId), async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(CacheSeconds);

            return await store.IsSessionActiveAsync(sessionId, timeProvider.GetUtcNow().UtcDateTime, cancellationToken);
        });

    public void Invalidate(Guid sessionId) => cache.Remove(CacheKey(sessionId));

    private static string CacheKey(Guid sessionId) => $"iam:session:{sessionId}";
}
