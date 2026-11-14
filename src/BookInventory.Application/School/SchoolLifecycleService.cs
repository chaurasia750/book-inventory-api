using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using BookInventory.Domain.School;
using SchoolRoot = BookInventory.Domain.School.School;

namespace BookInventory.Application.School;

/// <summary>
/// R2-R8: edits a school, replaces its class list and switches it off and on. Every rule is the aggregate's; this class
/// loads, calls one behaviour and lets the request transaction commit. A school of another company is not found (P19).
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class SchoolLifecycleService(ISchoolStore store)
{
    public async Task EditAsync(Guid id, EditSchoolRequest request, CancellationToken cancellationToken) =>
        (await LoadAsync(id, cancellationToken)).Edit(request.ToDetails());

    public async Task ReplaceClassesAsync(Guid id, ReplaceClassesRequest request, CancellationToken cancellationToken) =>
        (await LoadAsync(id, cancellationToken)).ReplaceClasses(request.Classes);

    public async Task DeactivateAsync(Guid id, CancellationToken cancellationToken) =>
        (await LoadAsync(id, cancellationToken)).Deactivate();

    public async Task ReactivateAsync(Guid id, CancellationToken cancellationToken) =>
        (await LoadAsync(id, cancellationToken)).Reactivate();

    private async Task<SchoolRoot> LoadAsync(Guid id, CancellationToken cancellationToken) =>
        await store.FindForManagementAsync(id, cancellationToken) ?? throw new NotFoundException(SchoolMessages.NotFound);
}
