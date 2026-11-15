using BookInventory.BuildingBlocks.Application;
using BookInventory.Domain.Sales;

namespace BookInventory.Application.Payment;

/// <summary>
/// Spec payment R12 (Q4): an invoice was cancelled (sales R12); what receipts had applied to it becomes advance of those receipts
/// (a negative allocation row each), in the same request transaction. No payment is lost and no ledger posting is needed: the
/// money is already in the school's credit.
/// </summary>
public sealed class InvoiceCancelledHandler(IReceiptStore store) : IDomainEventHandler<InvoiceCancelled>
{
    public async Task HandleAsync(InvoiceCancelled domainEvent, CancellationToken ct = default)
    {
        foreach (var receipt in await store.FindAppliedToInvoiceAsync(domainEvent.InvoiceId, ct))
            receipt.ReleaseInvoice(domainEvent.InvoiceId);
    }
}
