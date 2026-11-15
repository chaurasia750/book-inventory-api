using BookInventory.Application.Core;
using BookInventory.Application.Finance;
using BookInventory.Application.Price;
using BookInventory.Application.School;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.Domain.Finance;
using BookInventory.Domain.Payment;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Application.Payment;

/// <summary>
/// R4-R9: records a school payment. The use case reads the school and its open invoices, takes the next number of the company
/// series and hands them to the receipt (which validates the modes and plans the allocation); it then posts the receipt to the
/// school's ledger (R7) in the same request transaction. The same client request returns the receipt already recorded (R9).
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class ReceiptService(
    IReceiptStore store, PaymentDues dues, ISchoolDirectory schools, ILedgerPosting ledger, ICompanyCodes codes, BusinessCalendar calendar)
{
    public async Task<RecordedReceipt> RecordAsync(RecordReceiptRequest request, CancellationToken cancellationToken)
    {
        if (await store.FindIdByClientRequestAsync(request.ClientRequestId!.Value, cancellationToken) is { } existing)
            return new RecordedReceipt(existing, null);

        var school = await schools.GetAsync(request.SchoolId!.Value, cancellationToken) ?? throw new ValidationFailureException(PaymentMessages.SchoolNotFound);
        var today = calendar.Today();
        var date = request.ReceiptDate ?? today;
        var open = await dues.OpenInvoicesAsync(school.Id, cancellationToken);

        var financialYear = FinancialYear.Of(date);
        var number = Receipt.NumberOf(financialYear, await codes.NextNumberAsync(Receipt.NumberSeries(financialYear), cancellationToken));
        var receipt = Receipt.Record(new ReceiptDraft
        {
            ClientRequestId = request.ClientRequestId.Value,
            Number = number,
            FinancialYear = financialYear,
            SchoolId = school.Id,
            ReceiptDate = date,
            Today = today,
            Modes = [.. (request.Modes ?? []).Select(m => m.ToInput())],
            OpenInvoices = [.. open.Select(o => new OpenInvoice(o.InvoiceId, o.Due))],
            Allocations = request.Allocations?.Select(a => a.ToInput()).ToList(),
            Remark = request.Remark
        });

        await store.AddAsync(receipt, cancellationToken);
        await ledger.PostAsync(ReceiptLedgerEntry.For(receipt), cancellationToken);
        return new RecordedReceipt(receipt.Id, ReceiptResponses.Detail(receipt, school, open.ToDictionary(o => o.InvoiceId, o => o.Number)));
    }
}
