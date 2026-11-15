using System.Reflection;
using BookInventory.Application.Company;
using BookInventory.Application.Core;
using BookInventory.Application.School;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Application.Documents;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Application.Payment;

/// <summary>R17: a rendered receipt: the PDF bytes and the file name (the receipt number with <c>/</c> as <c>-</c>).</summary>
public sealed record ReceiptPdf(byte[] Content, string FileName);

/// <summary>
/// R17: hands a receipt over as an A5 PDF, made like the invoice PDF (sales R28-R33): the receipt, its school and the live company
/// identity fill the module's template through the template port and the platform renderer prints it (platform P25). Nothing is
/// stored: every call renders again.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class ReceiptPdfService(ReceiptQueryService queries, ISchoolDirectory schools, ICompanyInvoiceIdentity company, ITemplateRenderer templates, IHtmlToPdfRenderer renderer)
{
    private static readonly string Body = ReadTemplate("receipt-body.sbn");

    /// <summary>A5 portrait (5.83 x 8.27 in) with small margins; the company header is part of the body, so the page has no repeating header.</summary>
    private static readonly HtmlToPdfOptions Page = new()
    {
        PaperWidthInches = 5.83m, PaperHeightInches = 8.27m, MarginTopInches = 0.3m, MarginBottomInches = 0.3m, MarginLeftInches = 0.3m, MarginRightInches = 0.3m
    };

    public async Task<ReceiptPdf> RenderAsync(Guid id, CancellationToken cancellationToken)
    {
        var receipt = await queries.GetAsync(id, cancellationToken);
        var school = await schools.GetAsync(receipt.Summary.SchoolId, cancellationToken);
        var model = ReceiptPdfModelBuilder.Build(receipt, school, await company.CurrentAsync(cancellationToken));

        var html = await templates.RenderAsync(Body, model, cancellationToken);
        var pdf = await renderer.RenderAsync(html, Page, cancellationToken);
        return new ReceiptPdf(pdf, receipt.Summary.Number.Replace('/', '-') + ".pdf");
    }

    private static string ReadTemplate(string name)
    {
        var assembly = typeof(ReceiptPdfService).Assembly;
        var resource = assembly.GetManifestResourceNames().Single(n => n.EndsWith("." + name, StringComparison.Ordinal));
        using var stream = assembly.GetManifestResourceStream(resource)!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
