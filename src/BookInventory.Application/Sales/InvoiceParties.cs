using BookInventory.Application.Company;
using BookInventory.Application.School;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.Domain.Core;
using BookInventory.Domain.Sales;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Application.Sales;

/// <summary>
/// R13, R33: the school snapshot (with its address and GSTIN) and the company's printed identity an invoice keeps when it
/// is made, read through their capabilities. The identity is the company's at that moment; the logo and the signature are
/// left out on purpose (they stay live, R17/R18).
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class InvoiceParties(ISchoolDirectory schools, ICompanyInvoiceIdentity company)
{
    public async Task<(SaleSchool School, string? SchoolGstin, CompanyInvoiceSnapshot Company)> OfAsync(SchoolOrder order, CancellationToken cancellationToken)
    {
        var school = await schools.GetAsync(order.SchoolId, cancellationToken);
        var identity = await IdentityAsync(cancellationToken);

        var sale = new SaleSchool(
            order.SchoolId,
            school?.Code ?? order.SchoolCode,
            school?.Name ?? order.SchoolName,
            school?.State ?? order.SchoolState,
            school?.Address);

        return (sale, school?.Gstin, Snapshot(identity));
    }

    /// <summary>R13: the company's printed identity as it is now, kept on a counter bill like on an invoice (sales R13, R42).</summary>
    public async Task<CompanyInvoiceSnapshot> CompanySnapshotAsync(CancellationToken cancellationToken) =>
        Snapshot(await IdentityAsync(cancellationToken));

    private async Task<CompanyPrintedIdentity> IdentityAsync(CancellationToken cancellationToken) =>
        await company.CurrentAsync(cancellationToken) ?? throw new ForbiddenException(CoreMessages.CompanyRequired);

    private static CompanyInvoiceSnapshot Snapshot(CompanyPrintedIdentity identity) =>
        new(
            identity.Name, identity.Address, identity.Mobile, identity.Email, identity.Gstin, identity.Pan, identity.Tagline, identity.State,
            identity.Bank.AccountHolder, identity.Bank.BankName, identity.Bank.AccountNumber,
            identity.Bank.Ifsc, identity.Bank.Branch, identity.Bank.UpiId,
            identity.Invoice.Terms, identity.Invoice.Footer, identity.Invoice.PrintGstin);
}
