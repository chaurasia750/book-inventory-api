using BookInventory.Application.Company;
using BookInventory.Application.Core;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Application.Documents;
using BookInventory.Domain.Sales;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Application.Sales;

/// <summary>R28: a rendered invoice: the PDF bytes and the file name (the invoice number with <c>/</c> as <c>-</c>).</summary>
public sealed record InvoicePdf(byte[] Content, string FileName);

/// <summary>
/// R28-R33: hands an invoice over as an A4 PDF. It reads the invoice with the company identity it kept (R13), fills the three
/// module templates (the body, the header and the footer that repeat on every page) through the template port and has the
/// platform renderer print them (platform P25). Nothing is stored: every call renders again (Q18).
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class InvoicePdfService(
    InvoiceQueryService queries, ICompanyInvoiceIdentity company, ITemplateRenderer templates, IHtmlToPdfRenderer renderer)
{
    /// <summary>The header repeats on every page, so its height is the top margin; the footer is the bottom margin (R30).</summary>
    private static readonly HtmlToPdfOptions Page = new() { MarginTopInches = 1.55m, MarginBottomInches = 0.65m, MarginLeftInches = 0.3m, MarginRightInches = 0.3m };

    public async Task<InvoicePdf> RenderAsync(Guid id, InvoiceLayout? layout, CancellationToken cancellationToken)
    {
        var printable = await queries.PrintableAsync(id, layout, cancellationToken);
        var model = InvoicePdfModelBuilder.Build(printable, await company.CurrentAsync(cancellationToken));

        var body = await templates.RenderAsync(InvoicePdfTemplates.Body, model, cancellationToken);
        var header = await templates.RenderAsync(InvoicePdfTemplates.Header, model, cancellationToken);
        var footer = await templates.RenderAsync(InvoicePdfTemplates.Footer, model, cancellationToken);

        var pdf = await renderer.RenderAsync(body, Page with { HeaderHtml = header, FooterHtml = footer }, cancellationToken);
        return new InvoicePdf(pdf, printable.Detail.Summary.Number.Replace('/', '-') + ".pdf");
    }
}
