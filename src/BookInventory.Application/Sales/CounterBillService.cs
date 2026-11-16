using BookInventory.Application.Core;
using BookInventory.Application.Price;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.Domain.Finance;
using BookInventory.Domain.Sales;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Application.Sales;

/// <summary>
/// R42-R47, D13: makes a counter bill and cancels it. The counter must be open (R39); the student is found or created (R41); the
/// amounts are the one rule of R7 and the payments must equal the total (R45); the stock out is posted in the same transaction and
/// nothing goes to any ledger (R46), so the whole bill, the student and the number included, fails when anything is refused.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class CounterBillService(
    ICounterBillStore bills, ICounterStore counters, CounterBillComposer composer, ICompanyCodes codes, BusinessCalendar calendar, CounterBillPostings postings)
{
    public async Task<CounterBillMade> MakeAsync(MakeCounterBillRequest request, CancellationToken cancellationToken)
    {
        composer.RequireCompany();
        var counter = await counters.FindCounterForManagementAsync(request.CounterId!.Value, cancellationToken)
            ?? throw new NotFoundException(CounterMessages.CounterNotFound);
        counter.RecordBill();

        var today = calendar.Today();
        var billDate = request.BillDate ?? today;
        var financialYear = FinancialYear.Of(billDate);
        var number = CounterBill.NumberOf(financialYear, await codes.NextNumberAsync(CounterBill.NumberSeries(financialYear), cancellationToken));
        var bill = CounterBill.Make(await composer.ComposeAsync(counter, request, new CounterBillStamp(number, billDate, today), cancellationToken));
        await bills.AddAsync(bill, cancellationToken);

        var low = await postings.PostAsync(bill, cancellationToken);
        return new CounterBillMade(bill.Id, bill.Number, bill.Totals.GrandTotal, low);
    }

    public async Task CancelAsync(Guid id, CancelCounterBillRequest request, CancellationToken cancellationToken)
    {
        composer.RequireCompany();
        var bill = await bills.FindForManagementAsync(id, cancellationToken) ?? throw new NotFoundException(CounterMessages.BillNotFound);
        var today = calendar.Today();
        bill.Cancel(request.Reason ?? string.Empty, today);
        await postings.ReverseAsync(bill, today, cancellationToken);
    }
}
