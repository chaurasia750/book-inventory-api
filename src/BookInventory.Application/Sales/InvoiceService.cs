using BookInventory.Application.Company;
using BookInventory.Application.Core;
using BookInventory.Application.Finance;
using BookInventory.Application.Price;
using BookInventory.Application.School;
using BookInventory.Application.Stock;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.Domain.Finance;
using BookInventory.Domain.Sales;
using BookInventory.Domain.Stock;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Application.Sales;

/// <summary>
/// R8-R12, D3: makes an invoice from a confirmed order and cancels it. The order hands out the delivery; the invoice computes its
/// amounts; the stock out and the school ledger debit are posted through their capabilities in the same transaction, so the whole
/// invoice fails when a posting fails; a cancel reverses both and gives the delivery back to the order.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class InvoiceService(
    ISalesStore store, InvoicePostings postings, ICompanyCodes codes, InvoiceParties parties, BusinessCalendar calendar, ICreditNoteStore creditNotes)
{
    public async Task<InvoiceMade> MakeAsync(Guid orderId, MakeInvoiceRequest request, CancellationToken cancellationToken)
    {
        var order = await store.FindOrderForManagementAsync(orderId, cancellationToken) ?? throw new NotFoundException(SalesMessages.OrderNotFound);
        var today = calendar.Today();
        var invoiceDate = request.InvoiceDate ?? today;
        var quantities = request.Lines is { Count: > 0 }
            ? request.Lines.GroupBy(l => l.OrderLineId!.Value).ToDictionary(g => g.Key, g => g.Sum(l => l.Quantity!.Value))
            : order.Lines.ToDictionary(l => l.Id, l => l.RemainingQuantity);

        var delivery = order.Deliver(quantities);
        var (school, gstin, companySnapshot) = await parties.OfAsync(order, cancellationToken);
        var financialYear = Domain.Finance.FinancialYear.Of(invoiceDate);
        var number = Invoice.NumberOf(financialYear, await codes.NextNumberAsync(Invoice.NumberSeries(financialYear), cancellationToken));
        var invoice = Invoice.Issue(new InvoiceIssue
        {
            Number = number, InvoiceDate = invoiceDate, Today = today, Order = order, School = school, SchoolGstin = gstin,
            CompanyState = companySnapshot.State, CompanySnapshot = companySnapshot, Delivery = delivery
        });
        await store.AddAsync(invoice, cancellationToken);

        var low = await postings.PostAsync(invoice, cancellationToken);
        return new InvoiceMade(invoice.Id, invoice.Number, invoice.Totals.GrandTotal, low);
    }

    public async Task CancelAsync(Guid invoiceId, CancelInvoiceRequest request, CancellationToken cancellationToken)
    {
        var invoice = await store.FindInvoiceForManagementAsync(invoiceId, cancellationToken) ?? throw new NotFoundException(SalesMessages.InvoiceNotFound);
        if (await creditNotes.HasLiveForInvoiceAsync(invoiceId, cancellationToken))
            throw new ConflictException(ReturnMessages.InvoiceHasCreditNotes);

        var today = calendar.Today();
        invoice.Cancel(request.Reason, today);

        var order = await store.FindOrderForManagementAsync(invoice.SchoolOrderId, cancellationToken) ?? throw new NotFoundException(SalesMessages.OrderNotFound);
        order.GiveBack(invoice.Delivery);
        await postings.ReverseAsync(invoice, today, cancellationToken);
    }
}
