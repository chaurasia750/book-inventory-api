using BookInventory.BuildingBlocks.Application.Attributes;
using Microsoft.Extensions.DependencyInjection;
using SchoolRoot = BookInventory.Domain.School.School;

namespace BookInventory.Application.School;

/// <summary>
/// R1-R8, R15: creates a school in the company of the session. The code comes from the company's counter under its lock,
/// so parallel creations get different numbers; the company itself is stamped by the platform when the row is saved. Any
/// refused value throws and the request transaction saves nothing, counter included.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class SchoolRegistrationService(ISchoolStore store)
{
    public async Task<SchoolDetailResponse> CreateAsync(CreateSchoolRequest request, CancellationToken cancellationToken)
    {
        var counter = await store.LockCounterAsync(cancellationToken);
        var school = SchoolRoot.Create(counter.Next(), request.ToDetails());
        if (request.Classes is { Count: > 0 } classes)
            school.ReplaceClasses(classes);

        await store.AddAsync(school, cancellationToken);
        return SchoolResponses.Detail(school);
    }
}
