using BookInventory.Application.Price;
using BookInventory.BuildingBlocks.Application;
using BookInventory.Domain.Finance;
using BookInventory.Domain.School;

namespace BookInventory.Application.Finance;

/// <summary>
/// Spec finance R6 (school R17): a school's opening balance was set or changed; the difference is posted to the ledger in the same
/// request transaction (platform P24), dated the business date. A failing posting rolls the school's change back.
/// </summary>
public sealed class SchoolOpeningBalanceHandler(ILedgerPosting ledger, BusinessCalendar calendar) : IDomainEventHandler<SchoolOpeningBalanceChanged>
{
    public async Task HandleAsync(SchoolOpeningBalanceChanged domainEvent, CancellationToken ct = default)
    {
        var posting = OpeningBalanceEntry.For(
            new LedgerParty(PartyType.School, domainEvent.SchoolId),
            domainEvent.Code,
            new OpeningBalanceChange(domainEvent.OldAmount, domainEvent.NewAmount),
            calendar.Today());
        if (posting is not null)
            await ledger.PostAsync(posting, ct);
    }
}
