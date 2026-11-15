using BookInventory.Application.Finance;
using BookInventory.Application.Sales;
using BookInventory.Application.School;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.Domain.Finance;
using BookInventory.Domain.Payment;
using BookInventory.Domain.Sales;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Application.Payment;

/// <summary>
/// R3, R16: what a school still owes. An invoice's due is its grand total minus what receipts applied to it (the sum of the
/// allocation rows); an open invoice is an issued one with a due above zero. The invoices come from the sales invoice directory
/// (sales R37) and the balance from the ledger (finance R7); nothing is joined across modules.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class PaymentDues(IReceiptStore store, IInvoiceDirectory invoices, ISchoolDirectory schools, ILedgerBalances balances)
{
    public const int MaxInvoiceIds = 100;

    /// <summary>R3: the school's balance and its open invoices, oldest first; <c>404</c> for an unknown school.</summary>
    public async Task<DuesResponse> OfSchoolAsync(Guid schoolId, CancellationToken cancellationToken)
    {
        if (await schools.GetAsync(schoolId, cancellationToken) is null)
            throw new NotFoundException(PaymentMessages.SchoolNotFound);

        var balance = await balances.BalanceOfAsync(new LedgerParty(PartyType.School, schoolId), cancellationToken);
        return new DuesResponse(schoolId, balance, await OpenInvoicesAsync(schoolId, cancellationToken));
    }

    /// <summary>R3: the issued invoices of a school that still have a due, oldest first.</summary>
    public async Task<IReadOnlyList<OpenInvoiceDto>> OpenInvoicesAsync(Guid schoolId, CancellationToken cancellationToken)
    {
        var issued = await invoices.IssuedOfSchoolAsync(schoolId, cancellationToken);
        var allocated = await store.AllocatedAsync([.. issued.Select(i => i.Id)], cancellationToken);
        return
        [
            .. issued
                .Select(i => new OpenInvoiceDto(i.Id, i.Number, i.InvoiceDate, i.GrandTotal, allocated.GetValueOrDefault(i.Id), i.GrandTotal - allocated.GetValueOrDefault(i.Id)))
                .Where(o => o.Due > 0m)
        ];
    }

    /// <summary>R16: the allocated and due amounts of up to 100 invoices; an unknown id is left out and a cancelled invoice has no due.</summary>
    public async Task<IReadOnlyList<InvoiceDueDto>> OfInvoicesAsync(IReadOnlyCollection<Guid> invoiceIds, CancellationToken cancellationToken)
    {
        if (invoiceIds.Count > MaxInvoiceIds)
            throw new ValidationFailureException(PaymentMessages.TooManyIds);

        var found = await invoices.ReferencesAsync(invoiceIds, cancellationToken);
        var allocated = await store.AllocatedAsync([.. found.Keys], cancellationToken);
        return
        [
            .. found.Values.Select(i =>
            {
                var applied = allocated.GetValueOrDefault(i.Id);
                return new InvoiceDueDto(i.Id, i.GrandTotal, applied, i.Status == InvoiceStatus.Issued ? Math.Max(i.GrandTotal - applied, 0m) : 0m);
            })
        ];
    }
}
