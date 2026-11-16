using BookInventory.Application.Company;
using BookInventory.Application.Core;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Application.Documents;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.Domain.Purchase;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Application.Purchase;

/// <summary>R11: a rendered Debit Note: the PDF bytes and the file name (the number with <c>/</c> as <c>-</c>).</summary>
public sealed record DebitNotePdf(byte[] Content, string FileName);

/// <summary>
/// R11: hands a vendor return over as an A4 Debit Note PDF, made like the credit note (sales R71): the vendor return, its snapshot and
/// the live logo and signature fill the module's body, header and footer templates through the template port and the platform
/// renderer prints them (platform P25); the header and the footer repeat on every page. Nothing is stored; a cancelled vendor return
/// prints marked <c>CANCELLED</c>.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class DebitNotePdfService(IVendorReturnStore returns, ICompanyInvoiceIdentity company, ITemplateRenderer templates, IHtmlToPdfRenderer renderer)
{
    private static readonly string Body = Read("debit-note-body.sbn");
    private static readonly string Header = Read("debit-note-header.sbn");
    private static readonly string Footer = Read("debit-note-footer.sbn");

    /// <summary>The header repeats on every page, so its height is the top margin; the footer is the bottom margin.</summary>
    private static readonly HtmlToPdfOptions Page = new() { MarginTopInches = 1.18m, MarginBottomInches = 0.5m, MarginLeftInches = 0.3m, MarginRightInches = 0.3m };

    public async Task<DebitNotePdf> RenderAsync(Guid id, CancellationToken cancellationToken)
    {
        var vendorReturn = await returns.FindForReadAsync(id, cancellationToken) ?? throw new NotFoundException(PurchaseMessages.VendorReturnNotFound);
        var model = DebitNotePdfModelBuilder.Build(vendorReturn, await company.CurrentAsync(cancellationToken));

        var body = await templates.RenderAsync(Body, model, cancellationToken);
        var header = await templates.RenderAsync(Header, model, cancellationToken);
        var footer = await templates.RenderAsync(Footer, model, cancellationToken);

        var pdf = await renderer.RenderAsync(body, Page with { HeaderHtml = header, FooterHtml = footer }, cancellationToken);
        return new DebitNotePdf(pdf, vendorReturn.Number.Replace('/', '-') + ".pdf");
    }

    private static string Read(string name)
    {
        var assembly = typeof(DebitNotePdfService).Assembly;
        var resource = assembly.GetManifestResourceNames().Single(n => n.EndsWith("." + name, StringComparison.Ordinal));
        using var stream = assembly.GetManifestResourceStream(resource)!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
