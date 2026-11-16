using BookInventory.Application.Price;
using BookInventory.Application.School;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.Domain.Sales;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Application.Sales;

/// <summary>R53: the number, the date and today a credit note is stamped with before its lines are composed.</summary>
public sealed record CreditNoteStamp(string Number, DateOnly NoteDate, DateOnly Today);

/// <summary>
/// R53-R56: turns a credit note request into the domain's input, asking each module through its capability: the school (school R16,
/// R18), the company's printed identity (company R17-R20) and, for a return without an invoice, the items and prices through the order
/// composer (so the item-discount setting of R36 is the same rule) and the school's last rate (Q35). Every value rule is the aggregate's.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class CreditNoteComposer(
    PriceCompanyGuard companyGuard, ISchoolDirectory schools, ISchoolSessions sessions, InvoiceParties parties, OrderComposer orders, ICreditNoteStore notes)
{
    /// <summary>P20: a write needs a company in the session.</summary>
    public void RequireCompany() => companyGuard.Require();

    public async Task<CreditNoteInput> ComposeAsync(MakeCreditNoteRequest request, CreditNoteStamp stamp, Invoice? invoice, CancellationToken cancellationToken)
    {
        var school = await schools.GetAsync(request.SchoolId!.Value, cancellationToken) ?? throw new ValidationFailureException(ReturnMessages.SchoolNotFound);
        var company = await parties.CompanySnapshotAsync(cancellationToken);
        var isGst = invoice?.IsGstApplied ?? request.IsGstApplied ?? school.IsGstDefault;
        return new CreditNoteInput
        {
            Number = stamp.Number,
            NoteDate = stamp.NoteDate,
            Today = stamp.Today,
            School = new SaleSchool(school.Id, school.Code, school.Name, school.State, school.Address),
            SchoolGstin = school.Gstin,
            CompanySnapshot = company,
            Session = invoice?.Session ?? await SessionOnAsync(school.Id, stamp.NoteDate, cancellationToken),
            Invoice = invoice is null ? null : new ReturnedInvoice(invoice.Id, invoice.Number, invoice.InvoiceDate, invoice.Totals.Subtotal, new Discount(invoice.TotalDiscountKind, invoice.TotalDiscountValue)),
            IsGstApplied = isGst,
            GstMode = invoice?.GstMode ?? SaleMoney.GstModeOf(isGst, company.State, school.State),
            Reason = request.Reason!.Value,
            Remark = request.Remark,
            Lines = invoice is null
                ? await LinesWithoutInvoiceAsync(school.Id, stamp.NoteDate, request.Lines ?? [], cancellationToken)
                : LinesFromInvoice(invoice, request.Lines ?? [])
        };
    }

    /// <summary>R54 (FR-RET-02): each line is one line of the invoice, at the invoice's rate and item discount.</summary>
    private static List<CreditNoteLineInput> LinesFromInvoice(Invoice invoice, IReadOnlyList<CreditNoteLineRequest> requested)
    {
        var lines = invoice.Lines.ToDictionary(l => l.Id);
        return [.. requested.Select(r => lines.TryGetValue(r.InvoiceLineId ?? Guid.Empty, out var line)
            ? CreditNoteLineInput.OfInvoiceLine(line, r.Quantity!.Value, r.Condition!.Value)
            : throw new ValidationFailureException(ReturnMessages.InvoiceLineUnknown))];
    }

    /// <summary>R54, Q35: an item at the school's last invoiced price, else the item's default price version; the rate may be changed.</summary>
    private async Task<List<CreditNoteLineInput>> LinesWithoutInvoiceAsync(
        Guid schoolId, DateOnly noteDate, IReadOnlyList<CreditNoteLineRequest> requested, CancellationToken cancellationToken)
    {
        if (requested.Any(r => r.ItemId is null))
            throw new ValidationFailureException(ReturnMessages.InvoiceLineUnknown);

        var asked = requested.Select(r => new SchoolOrderLineRequest(r.ItemId, null, r.Quantity, r.Discount)).ToList();
        var composed = await orders.LinesAsync(new OrderLinesRequest(schoolId, noteDate, asked), cancellationToken);
        var last = await notes.LastInvoicedPricesAsync(schoolId, [.. composed.Select(c => c.Item.Id)], cancellationToken);
        return [.. composed.Select((c, i) => new CreditNoteLineInput(c.Item, PriceOf(c.Price, last.GetValueOrDefault(c.Item.Id), requested[i].Rate), c.Quantity, requested[i].Condition!.Value, c.ItemDiscount))];
    }

    private static SalePrice PriceOf(SalePrice dflt, LastInvoicedPrice? last, decimal? rate)
    {
        var basis = last is null ? dflt : new SalePrice(last.PriceVersionId, last.PriceVersionName, last.MrpAmount, last.RateAmount);
        return basis with { Rate = rate ?? basis.Rate };
    }

    private async Task<string> SessionOnAsync(Guid schoolId, DateOnly date, CancellationToken cancellationToken) =>
        (await sessions.SessionOfAsync(schoolId, date, cancellationToken) ?? throw new ValidationFailureException(ReturnMessages.SchoolNotFound)).Session;
}
