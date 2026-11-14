namespace BookInventory.Application.Iam.Companies;

/// <summary>
/// R69: the capability the <c>company</c> module calls when a Super Admin enters or leaves a company. The chosen company
/// is stored on the caller's own session and ends with it; only a <c>SUPER_ADMIN</c> session can carry one. The
/// <c>company</c> module has already checked that the company exists and is active.
/// </summary>
public interface ISessionCompanySelector
{
    Task EnterAsync(Guid companyId, CancellationToken cancellationToken = default);

    Task LeaveAsync(CancellationToken cancellationToken = default);
}
