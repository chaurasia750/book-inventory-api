namespace BookInventory.Domain.Sales;

/// <summary>
/// Spec sales R13, R33, D7 (owner's answers Q17): the company's printed identity as it was when the invoice was made, so a
/// reprint never changes when the company later edits its profile. The logo and the signature are not kept here: they are
/// read live from the company (cosmetic, R17/R18).
/// </summary>
public sealed record CompanyInvoiceSnapshot(
    string Name,
    string? Address,
    string? Mobile,
    string? Email,
    string? Gstin,
    string? Pan,
    string? Tagline,
    string? State,
    string? BankAccountHolder,
    string? BankName,
    string? BankAccountNumber,
    string? BankIfsc,
    string? BankBranch,
    string? UpiId,
    string? Terms,
    string? Footer,
    bool PrintGstin)
{
    /// <summary>An empty identity; the shape of an invoice made before this snapshot existed (none in production).</summary>
    public static readonly CompanyInvoiceSnapshot Empty =
        new(string.Empty, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, true);
}
