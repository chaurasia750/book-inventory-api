namespace BookInventory.Application.Iam.Companies;

/// <summary>
/// R63: the capability the <c>company</c> module calls when it deactivates a company. Every usable session of every
/// account of that company ends at once, so the next request of each of them answers 401 (as R31 does for one user).
/// It runs in the caller's transaction and is a plain call, not an event handler: handlers run after the save.
/// </summary>
public interface ICompanySessionRevoker
{
    Task RevokeAllAsync(Guid companyId, CancellationToken cancellationToken = default);
}
