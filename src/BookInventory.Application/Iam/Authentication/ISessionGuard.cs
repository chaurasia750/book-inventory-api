namespace BookInventory.Application.Iam.Authentication;

/// <summary>
/// R31 groundwork: the access token of a revoked session must stop working on the very next request, not when it
/// happens to expire. The check is a database read per request, so the answer is cached for a moment and dropped as
/// soon as something revokes the session.
/// </summary>
public interface ISessionGuard
{
    /// <summary>True while the session exists, is not revoked and has not run out.</summary>
    Task<bool> IsActiveAsync(Guid sessionId, CancellationToken cancellationToken = default);

    /// <summary>Forgets the cached answer, so a revoke takes effect immediately instead of after the cache window.</summary>
    void Invalidate(Guid sessionId);
}
