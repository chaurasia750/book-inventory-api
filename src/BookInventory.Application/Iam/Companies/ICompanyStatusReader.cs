namespace BookInventory.Application.Iam.Companies;

/// <summary>
/// R63: whether a company may be used. A port defined here and implemented by the <c>company</c> module, so <c>iam</c>
/// never references the company's types. Login of a company account asks it; when no module provides it, a company
/// account is refused (fail closed).
/// </summary>
public interface ICompanyStatusReader
{
    Task<bool> IsActiveAsync(Guid companyId, CancellationToken cancellationToken = default);
}
