using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.BuildingBlocks.Domain.Models;
using Microsoft.Extensions.DependencyInjection;
using BookInventory.Domain.School;
using SchoolRoot = BookInventory.Domain.School.School;

namespace BookInventory.Application.School;

/// <summary>
/// R9, R11: the school list and detail. The company comes from the platform filter; the only rule applied here is the
/// Staff view: Staff see active schools only, and an inactive school is for them an unknown id.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class SchoolQueryService(ISchoolStore store, ICurrentUser currentUser)
{
    private const string StaffRole = "STAFF";

    public Task<IPagedList<SchoolSummary>> SearchAsync(SchoolSearchQuery query, CancellationToken cancellationToken) =>
        store.SearchAsync(query, activeOnly: IsStaff, cancellationToken);

    public async Task<SchoolDetailResponse> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var school = await store.FindForReadAsync(id, cancellationToken);
        if (school is null || (IsStaff && !school.IsActive))
            throw new NotFoundException(SchoolMessages.NotFound);

        return SchoolResponses.Detail(school);
    }

    private bool IsStaff => currentUser.Role == StaffRole;
}

/// <summary>The one projection of a school into its detail response, used by the create and the read use cases.</summary>
internal static class SchoolResponses
{
    public static SchoolDetailResponse Detail(SchoolRoot school) =>
        new(
            school.Id, school.Code, school.Name, school.Address, school.ContactPerson, school.Mobile, school.Email, school.Gstin,
            school.State, school.SessionType, school.SaleType, school.PaymentType, school.IsGstDefault, school.OpeningBalanceAmount, school.Status,
            school.CreatedAt, school.UpdatedAt,
            [.. school.Classes.OrderBy(c => c.SortOrder).Select(c => new SchoolClassResponse(c.Id, c.Name, c.SortOrder))]);
}
