using BookInventory.Application.Price;
using BookInventory.BuildingBlocks.Application;
using BookInventory.Domain.Finance;
using BookInventory.Domain.Vendor;

namespace BookInventory.Application.Finance;

/// <summary>
/// Spec finance R6 (vendor R16): a vendor's opening balance was set or changed; the difference is posted to the ledger in the same
/// request transaction (platform P24), dated the business date. A failing posting rolls the vendor's change back.
/// </summary>
public sealed class VendorOpeningBalanceHandler(ILedgerPosting ledger, BusinessCalendar calendar) : IDomainEventHandler<VendorOpeningBalanceChanged>
{
    public async Task HandleAsync(VendorOpeningBalanceChanged domainEvent, CancellationToken ct = default)
    {
        var posting = OpeningBalanceEntry.For(
            new LedgerParty(PartyType.Vendor, domainEvent.VendorId),
            domainEvent.Code,
            new OpeningBalanceChange(domainEvent.OldAmount, domainEvent.NewAmount),
            calendar.Today());
        if (posting is not null)
            await ledger.PostAsync(posting, ct);
    }
}
