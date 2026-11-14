using BookInventory.BuildingBlocks.Application.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Application.Price;

/// <summary>
/// R4: "today" for the business is the date in the time zone <c>Asia/Kolkata</c> ([specs/company.md](company.md) D7), whatever
/// the server's zone is. The time comes from <see cref="TimeProvider"/> so a check can fix it.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class BusinessCalendar(TimeProvider timeProvider)
{
    private static readonly TimeZoneInfo Zone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");

    public DateOnly Today() => DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(timeProvider.GetUtcNow(), Zone).DateTime);
}
