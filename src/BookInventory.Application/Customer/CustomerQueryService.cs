using BookInventory.Application.School;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.BuildingBlocks.Domain.Models;
using BookInventory.Domain.Customer;
using Microsoft.Extensions.DependencyInjection;
using CustomerRoot = BookInventory.Domain.Customer.Customer;

namespace BookInventory.Application.Customer;

/// <summary>
/// R8, R11: the customer list, detail and suggestion. The company comes from the platform filter; the roles are checked on the
/// controller. The school's name is read through the school directory (school R16), never by joining its tables.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class CustomerQueryService(ICustomerStore store, ISchoolDirectory schools)
{
    public async Task<IPagedList<CustomerSummary>> SearchAsync(CustomerSearchQuery query, CancellationToken cancellationToken)
    {
        var page = await store.SearchAsync(query, cancellationToken);
        var names = await SchoolNamesAsync(page.Items.Select(r => r.SchoolId), cancellationToken);
        return new PagedList<CustomerSummary>
        {
            PageIndex = page.PageIndex,
            PageSize = page.PageSize,
            IndexFrom = page.IndexFrom,
            TotalCount = page.TotalCount,
            TotalPages = page.TotalPages,
            Items =
            [
                .. page.Items.Select(r => new CustomerSummary(
                    r.Id, r.Code, r.Name, r.Mobile, r.SchoolId, NameOf(names, r.SchoolId), r.ClassName, r.Status, r.CreatedAt))
            ]
        };
    }

    public async Task<IReadOnlyList<CustomerSuggestion>> SuggestAsync(string? text, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(text) || text.Trim().Length < 2)
            return [];

        var rows = await store.SuggestAsync(text, cancellationToken);
        var names = await SchoolNamesAsync(rows.Select(r => r.SchoolId), cancellationToken);
        return [.. rows.Select(r => new CustomerSuggestion(r.Id, r.Code, r.Name, r.Mobile, NameOf(names, r.SchoolId)))];
    }

    public async Task<CustomerDetailResponse> GetAsync(Guid id, CancellationToken cancellationToken) =>
        await DetailOfAsync(
            await store.FindForReadAsync(id, cancellationToken) ?? throw new NotFoundException(CustomerMessages.NotFound),
            cancellationToken);

    /// <summary>The one projection of a customer into its detail response, used by the create and the read use cases.</summary>
    public async Task<CustomerDetailResponse> DetailOfAsync(CustomerRoot customer, CancellationToken cancellationToken)
    {
        var names = await SchoolNamesAsync([customer.SchoolId], cancellationToken);
        return new CustomerDetailResponse(
            customer.Id, customer.Code, customer.Name, customer.Mobile, customer.Address, customer.SchoolId,
            NameOf(names, customer.SchoolId), customer.ClassName, customer.OpeningBalanceAmount, customer.CreditLimitAmount,
            customer.Status, customer.CreatedAt, customer.UpdatedAt);
    }

    private async Task<IReadOnlyDictionary<Guid, SchoolReference>> SchoolNamesAsync(
        IEnumerable<Guid?> schoolIds, CancellationToken cancellationToken)
    {
        var ids = schoolIds.OfType<Guid>().Distinct().ToList();
        return await schools.ReferencesAsync(ids, cancellationToken);
    }

    private static string? NameOf(IReadOnlyDictionary<Guid, SchoolReference> schools, Guid? schoolId) =>
        schoolId is { } id && schools.TryGetValue(id, out var school) ? school.Name : null;
}
