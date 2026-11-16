using BookInventory.Application.Finance;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.Domain.Finance;
using BookInventory.Domain.Sales;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Application.Sales;

/// <summary>R66, R67: the refund's ledger transaction (finance R5) and its reversal on a cancel. A refund moves no stock.</summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class RefundPostings(ILedgerPosting ledger)
{
    public Task PostAsync(SchoolRefund refund, CancellationToken cancellationToken) =>
        ledger.PostAsync(RefundLedgerEntry.For(refund), cancellationToken);

    public Task ReverseAsync(SchoolRefund refund, DateOnly today, CancellationToken cancellationToken) =>
        ledger.ReverseAsync(new LedgerDocument(SchoolRefund.DocumentType, refund.Id, refund.Number), today, cancellationToken);
}
