using BookInventory.Application.Core;
using BookInventory.Application.Price;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.Domain.Finance;
using BookInventory.Domain.Sales;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Application.Sales;

/// <summary>R64: the session company check, today's business date and the next refund number (from the company code series, platform P23).</summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class RefundStamps(PriceCompanyGuard companyGuard, ICompanyCodes codes, BusinessCalendar calendar)
{
    /// <summary>P20: a write needs a company in the session.</summary>
    public void RequireCompany() => companyGuard.Require();

    public DateOnly Today() => calendar.Today();

    /// <summary>The number of the next refund of the financial year of <paramref name="requestedDate"/> (today when empty), with that date and today.</summary>
    public async Task<(string Number, DateOnly Date, DateOnly Today)> NewAsync(DateOnly? requestedDate, CancellationToken cancellationToken)
    {
        var today = calendar.Today();
        var date = requestedDate ?? today;
        var year = FinancialYear.Of(date);
        var number = SchoolRefund.NumberOf(year, await codes.NextNumberAsync(SchoolRefund.NumberSeries(year), cancellationToken));
        return (number, date, today);
    }
}
