using BookInventory.Application.Iam.Authentication;

namespace BookInventory.Application.Iam.Companies;

/// <summary>What changed on the caller's session: the company it had before (null when none) and the access token that now carries the new one (R62).</summary>
public sealed record SessionCompanyChange(Guid? PreviousCompanyId, IssuedAccessToken AccessToken);

/// <summary>
/// R69: the capability the <c>company</c> module calls when a Super Admin enters or leaves a company. The chosen company
/// is stored on the caller's own session and ends with it; only a <c>SUPER_ADMIN</c> session can carry one (anything else
/// is refused with 403). The <c>company</c> module has already checked that the company exists and is active.
/// </summary>
public interface ISessionCompanySelector
{
    Task<SessionCompanyChange> EnterAsync(Guid companyId, CancellationToken cancellationToken = default);

    Task<SessionCompanyChange> LeaveAsync(CancellationToken cancellationToken = default);
}
