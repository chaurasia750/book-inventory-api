using BookInventory.Application.Company;
using BookInventory.Application.Core;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Application.Documents;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.Domain.Sales;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Application.Sales;

/// <summary>R51: a rendered counter bill receipt: the PDF bytes and the file name (the bill number with <c>/</c> as <c>-</c>).</summary>
public sealed record CounterBillPdf(byte[] Content, string FileName);

/// <summary>
/// R51: hands a counter bill over as an A5 PDF, made like the payment receipt (payment R17) and the invoice (sales R28-R33): the bill,
/// its snapshots and the live logo and signature fill the module's template through the template port and the platform renderer
/// prints it (platform P25). Nothing is stored: every call renders again; a cancelled bill prints marked <c>CANCELLED</c>.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class CounterBillPdfService(ICounterBillStore bills, ICounterStore counters, ICompanyInvoiceIdentity company, ITemplateRenderer templates, IHtmlToPdfRenderer renderer)
{
    private static readonly string Body = ReadTemplate("counter-bill-body.sbn");

    /// <summary>A5 portrait (5.83 x 8.27 in) with small margins; the company header is part of the body, so the page has no repeating header.</summary>
    private static readonly HtmlToPdfOptions Page = new()
    {
        PaperWidthInches = 5.83m, PaperHeightInches = 8.27m, MarginTopInches = 0.3m, MarginBottomInches = 0.3m, MarginLeftInches = 0.3m, MarginRightInches = 0.3m
    };

    public async Task<CounterBillPdf> RenderAsync(Guid id, CancellationToken cancellationToken)
    {
        var bill = await bills.FindForReadAsync(id, cancellationToken) ?? throw new NotFoundException(CounterMessages.BillNotFound);
        var counter = await counters.FindCounterForReadAsync(bill.CounterId, cancellationToken);
        var model = CounterBillPdfModelBuilder.Build(bill, counter?.Name, await company.CurrentAsync(cancellationToken));

        var html = await templates.RenderAsync(Body, model, cancellationToken);
        var pdf = await renderer.RenderAsync(html, Page, cancellationToken);
        return new CounterBillPdf(pdf, bill.Number.Replace('/', '-') + ".pdf");
    }

    private static string ReadTemplate(string name)
    {
        var assembly = typeof(CounterBillPdfService).Assembly;
        var resource = assembly.GetManifestResourceNames().Single(n => n.EndsWith("." + name, StringComparison.Ordinal));
        using var stream = assembly.GetManifestResourceStream(resource)!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
