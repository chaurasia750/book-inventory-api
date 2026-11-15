namespace BookInventory.Application.Company;

/// <summary>R16: the state and GSTIN of the session's company, for the GST of other modules' documents.</summary>
public sealed record CompanyTaxDetails(string? State, string? Gstin);

/// <summary>
/// R16: the capability other modules (the sales module first) use to read the session company's tax profile, never by joining the
/// company tables. It changes nothing.
/// </summary>
public interface ICompanyTaxProfile
{
    /// <summary>The session company's state and GSTIN, or <c>null</c> when the session has no company.</summary>
    Task<CompanyTaxDetails?> CurrentAsync(CancellationToken cancellationToken = default);
}
