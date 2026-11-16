using BookInventory.Application.Company;
using BookInventory.Application.Core;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Application.Documents;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.Domain.Sales;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Application.Sales;

/// <summary>R71: a rendered credit note: the PDF bytes and the file name (the credit note number with <c>/</c> as <c>-</c>).</summary>
public sealed record CreditNotePdf(byte[] Content, string FileName);

/// <summary>
/// R71: hands a credit note over as an A4 PDF, made like the invoice (sales R28-R33): the credit note, its snapshot and the live logo
/// and signature fill the module's body, header and footer templates through the template port and the platform renderer prints
/// them (platform P25); the header and the footer repeat on every page. Nothing is stored: every call renders again; a cancelled
/// credit note prints marked <c>CANCELLED</c>.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class CreditNotePdfService(ICreditNoteStore notes, ICompanyInvoiceIdentity company, ITemplateRenderer templates, IHtmlToPdfRenderer renderer)
{
    private static readonly string Body = Read("credit-note-body.sbn");
    private static readonly string Header = Read("credit-note-header.sbn");
    private static readonly string Footer = Read("credit-note-footer.sbn");

    /// <summary>The header repeats on every page, so its height is the top margin; the footer is the bottom margin (as the invoice).</summary>
    private static readonly HtmlToPdfOptions Page = new() { MarginTopInches = 1.55m, MarginBottomInches = 0.65m, MarginLeftInches = 0.3m, MarginRightInches = 0.3m };

    public async Task<CreditNotePdf> RenderAsync(Guid id, CancellationToken cancellationToken)
    {
        var note = await notes.FindForReadAsync(id, cancellationToken) ?? throw new NotFoundException(ReturnMessages.CreditNoteNotFound);
        var model = CreditNotePdfModelBuilder.Build(note, await company.CurrentAsync(cancellationToken));

        var body = await templates.RenderAsync(Body, model, cancellationToken);
        var header = await templates.RenderAsync(Header, model, cancellationToken);
        var footer = await templates.RenderAsync(Footer, model, cancellationToken);

        var pdf = await renderer.RenderAsync(body, Page with { HeaderHtml = header, FooterHtml = footer }, cancellationToken);
        return new CreditNotePdf(pdf, note.Number.Replace('/', '-') + ".pdf");
    }

    private static string Read(string name)
    {
        var assembly = typeof(CreditNotePdfService).Assembly;
        var resource = assembly.GetManifestResourceNames().Single(n => n.EndsWith("." + name, StringComparison.Ordinal));
        using var stream = assembly.GetManifestResourceStream(resource)!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
