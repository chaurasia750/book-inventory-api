using BookInventory.Application.Finance;
using BookInventory.Application.Price;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.Domain.Finance;
using BookInventory.Domain.Payment;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Application.Payment;

/// <summary>
/// R13, R14: applies a receipt's advance to the school's open invoices (nothing is posted: the money is already in the school's
/// credit) and cancels a receipt (the ledger transaction of the receipt is reversed, finance R5, and its allocations are taken back).
/// Every rule is the receipt's; this class loads, reads the open invoices, calls one behaviour and posts.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class ReceiptLifecycleService(IReceiptStore store, PaymentDues dues, ILedgerPosting ledger, BusinessCalendar calendar)
{
    public async Task AdjustAdvanceAsync(Guid receiptId, AdjustAdvanceRequest request, CancellationToken cancellationToken)
    {
        var receipt = await LoadAsync(receiptId, cancellationToken);
        var open = await dues.OpenInvoicesAsync(receipt.SchoolId, cancellationToken);
        receipt.AdjustAdvance(
            [.. open.Select(o => new OpenInvoice(o.InvoiceId, o.Due))],
            request.Allocations?.Select(a => a.ToInput()).ToList());
    }

    public async Task CancelAsync(Guid receiptId, CancelReceiptRequest request, CancellationToken cancellationToken)
    {
        var receipt = await LoadAsync(receiptId, cancellationToken);
        var today = calendar.Today();
        receipt.Cancel(request.Reason, today);
        await ledger.ReverseAsync(new LedgerDocument(ReceiptLedgerEntry.DocumentType, receipt.Id, receipt.Number), today, cancellationToken);
    }

    private async Task<Receipt> LoadAsync(Guid receiptId, CancellationToken cancellationToken) =>
        await store.FindForManagementAsync(receiptId, cancellationToken) ?? throw new NotFoundException(PaymentMessages.ReceiptNotFound);
}
