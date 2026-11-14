namespace BookInventory.Application.Iam.Companies;

/// <summary>
/// R62: the company of the current request, taken from the access token claim <c>cid</c> and never from request input.
/// A company account has its own company; a Super Admin has the company they entered, or none. Other modules read the
/// company only through this port.
/// </summary>
public interface ICurrentCompany
{
    /// <summary>The company of this request, or null when the caller has no company context.</summary>
    Guid? CompanyId { get; }
}
