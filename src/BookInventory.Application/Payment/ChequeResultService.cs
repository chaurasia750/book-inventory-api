using BookInventory.Application.Finance;
using BookInventory.Application.Price;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.Domain.Payment;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Application.Payment;

/// <summary>
/// R10, R11: sets the result of a cheque. The receipt validates the status and the dates and, for a bounce, takes the amount back
/// from the advance and the newest allocations; the use case then posts the ledger transaction of that result (finance R5) in the
/// same request transaction.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class ChequeResultService(IReceiptStore store, ILedgerPosting ledger, BusinessCalendar calendar)
{
    public async Task ClearAsync(Guid receiptId, Guid modeId, ClearChequeRequest request, CancellationToken cancellationToken)
    {
        var receipt = await LoadAsync(receiptId, cancellationToken);
        var cheque = receipt.ClearCheque(modeId, request.Date!.Value, calendar.Today());
        await ledger.PostAsync(ChequeLedgerEntry.Cleared(receipt, cheque), cancellationToken);
    }

    public async Task BounceAsync(Guid receiptId, Guid modeId, BounceChequeRequest request, CancellationToken cancellationToken)
    {
        var receipt = await LoadAsync(receiptId, cancellationToken);
        var cheque = receipt.BounceCheque(modeId, request.Date!.Value, calendar.Today(), request.Reason);
        await ledger.PostAsync(ChequeLedgerEntry.Bounced(receipt, cheque), cancellationToken);
    }

    private async Task<Receipt> LoadAsync(Guid receiptId, CancellationToken cancellationToken) =>
        await store.FindForManagementAsync(receiptId, cancellationToken) ?? throw new NotFoundException(PaymentMessages.ReceiptNotFound);
}
