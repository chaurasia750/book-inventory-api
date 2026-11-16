using BookInventory.Application.School;
using BookInventory.Domain.Sales;

namespace BookInventory.Application.Sales;

/// <summary>The one projection of a counter into its response, used by the open and the read use cases.</summary>
internal static class CounterResponses
{
    public static CounterResponse Of(Counter counter, SchoolReference? school) =>
        new(
            counter.Id, counter.Name, counter.SchoolId, school?.Code, school?.Name, counter.Session, counter.StartDate, counter.EndDate,
            counter.Status, counter.HasBills, counter.CreatedAt);
}
