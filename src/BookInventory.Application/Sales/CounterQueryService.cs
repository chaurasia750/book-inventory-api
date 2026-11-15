using BookInventory.Application.School;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.BuildingBlocks.Domain.Models;
using BookInventory.Domain.Sales;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Application.Sales;

/// <summary>
/// R49: the counter list and detail. The company comes from the platform filter; the roles are checked on the controller. The school's
/// name is read through the school directory (school R16), never by joining its tables.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class CounterQueryService(ICounterStore store, ISchoolDirectory schools)
{
    public async Task<IPagedList<CounterResponse>> SearchAsync(CounterSearchQuery query, CancellationToken cancellationToken)
    {
        var matched = string.IsNullOrWhiteSpace(query.Search) ? [] : await schools.IdsMatchingAsync(query.Search, cancellationToken);
        var page = await store.SearchCountersAsync(query, matched, cancellationToken);
        var references = await schools.ReferencesAsync([.. page.Items.Select(c => c.SchoolId).Distinct()], cancellationToken);
        return new PagedList<CounterResponse>
        {
            PageIndex = page.PageIndex,
            PageSize = page.PageSize,
            IndexFrom = page.IndexFrom,
            TotalCount = page.TotalCount,
            TotalPages = page.TotalPages,
            Items = [.. page.Items.Select(c => CounterResponses.Of(c, references.GetValueOrDefault(c.SchoolId)))]
        };
    }

    public async Task<CounterResponse> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var counter = await store.FindCounterForReadAsync(id, cancellationToken) ?? throw new NotFoundException(CounterMessages.CounterNotFound);
        return CounterResponses.Of(counter, await schools.GetAsync(counter.SchoolId, cancellationToken));
    }
}
