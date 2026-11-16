using System.Globalization;
using BookInventory.Application.Company;
using BookInventory.Application.Core;
using BookInventory.Application.School;
using BookInventory.Application.Vendor;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Application.Documents;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.Domain.Finance;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Application.Finance;

/// <summary>
/// R18: hands a party's ledger statement over as an A4 portrait PDF, made like the invoice PDF (sales R28-R33). The statement is
/// asked from the same use case as the screen (R9), so the PDF and the screen can never disagree and the same refusals apply
/// (an unknown party is <c>404</c>, more than 5000 lines is <c>400</c>). Nothing is stored: every call renders again.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class LedgerPdfService(
    LedgerStatementService statements, ISchoolDirectory schools, IVendorDirectory vendors, ICompanyInvoiceIdentity company, ITemplateRenderer templates, IHtmlToPdfRenderer renderer)
{
    private static readonly TimeZoneInfo Zone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");

    /// <summary>A4 portrait; the top margin holds the company header (R20) and the bottom margin the footer (made-at time and page numbers), both repeated on every page.</summary>
    private static readonly HtmlToPdfOptions Page = new() { MarginTopInches = 1.0m, MarginBottomInches = 0.6m, MarginLeftInches = 0.3m, MarginRightInches = 0.3m };

    public async Task<LedgerPdf> SchoolAsync(Guid schoolId, LedgerStatementQuery query, CancellationToken cancellationToken)
    {
        var statement = await statements.SchoolStatementAsync(schoolId, query, cancellationToken);
        var school = await schools.GetAsync(schoolId, cancellationToken) ?? throw new NotFoundException(FinanceMessages.PartyNotFound);
        var party = new LedgerPdfParty(school.Name, school.Code, school.Address, school.State, school.Gstin);
        return await RenderAsync(statement, party, query, cancellationToken);
    }

    public async Task<LedgerPdf> VendorAsync(Guid vendorId, LedgerStatementQuery query, CancellationToken cancellationToken)
    {
        var statement = await statements.VendorStatementAsync(vendorId, query, cancellationToken);
        var vendor = await vendors.GetAsync(vendorId, cancellationToken) ?? throw new NotFoundException(FinanceMessages.PartyNotFound);
        var party = new LedgerPdfParty(vendor.Name, vendor.Code, null, vendor.State, vendor.Gstin);
        return await RenderAsync(statement, party, query, cancellationToken);
    }

    private async Task<LedgerPdf> RenderAsync(LedgerStatement statement, LedgerPdfParty party, LedgerStatementQuery query, CancellationToken cancellationToken)
    {
        var madeAt = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, Zone).ToString("dd MMM yyyy HH:mm", CultureInfo.InvariantCulture);
        var heading = new LedgerPdfHeading(LedgerFilterWords.Of(query), LedgerPeriod.Of(query, statement.Rows), madeAt);
        var model = LedgerPdfModelBuilder.Build(statement, party, heading, await company.CurrentAsync(cancellationToken));
        var body = await templates.RenderAsync(LedgerPdfTemplates.Body, model, cancellationToken);
        var header = await templates.RenderAsync(LedgerPdfTemplates.Header, model, cancellationToken);
        var footer = await templates.RenderAsync(LedgerPdfTemplates.Footer, model, cancellationToken);
        var pdf = await renderer.RenderAsync(body, Page with { HeaderHtml = header, FooterHtml = footer }, cancellationToken);
        return new LedgerPdf(pdf, party.Code + "-ledger.pdf");
    }
}
