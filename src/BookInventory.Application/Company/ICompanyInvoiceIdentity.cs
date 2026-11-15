using BookInventory.Domain.Company;

namespace BookInventory.Application.Company;

/// <summary>R20: the session company's printed identity - its name, address and contact, its logo and signature images, its one bank account and its invoice settings.</summary>
public sealed record CompanyPrintedIdentity(
    string Name,
    string? Address,
    string? Mobile,
    string? Email,
    string? Gstin,
    string? State,
    string? Pan,
    string? Tagline,
    byte[]? Logo,
    string? LogoContentType,
    byte[]? Signature,
    string? SignatureContentType,
    CompanyBankDetails Bank,
    CompanyInvoiceSettings Invoice);

/// <summary>
/// R20: the capability other modules (the sales module first, for the invoice PDF) use to read the session company's
/// printed identity - the logo, the signature, the one bank account and the invoice settings - never by joining the
/// company tables. It changes nothing and answers <c>null</c> when the session has no company (the same shape as R16).
/// </summary>
public interface ICompanyInvoiceIdentity
{
    /// <summary>The session company's printed identity, or <c>null</c> when the session has no company.</summary>
    Task<CompanyPrintedIdentity?> CurrentAsync(CancellationToken cancellationToken = default);

    /// <summary>R23: whether the session company uses a discount on each line; <c>false</c> when it never set it or the session has no company.</summary>
    Task<bool> UsesItemDiscountAsync(CancellationToken cancellationToken = default);
}
