using BookInventory.Application.School;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Persistence.Contracts;
using BookInventory.Domain.School;
using Microsoft.EntityFrameworkCore;
using SchoolRoot = BookInventory.Domain.School.School;

namespace BookInventory.Infrastructure.School;

/// <summary>
/// Spec school R16: the read capability other modules use, over the same company-filtered repository as the school store, so it
/// only ever sees the session's company. Nothing is tracked and nothing is changed. The class comparison is the one of R6.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped, typeof(ISchoolDirectory))]
public sealed class SchoolDirectory(IUnitOfWork unitOfWork) : ISchoolDirectory
{
    private readonly IRepository<SchoolRoot> _schools = unitOfWork.Repository<SchoolRoot>();

    public async Task<SchoolReference?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        (await ReferencesAsync([id], cancellationToken)).GetValueOrDefault(id);

    public async Task<IReadOnlyDictionary<Guid, SchoolReference>> ReferencesAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default)
    {
        if (ids.Count == 0)
            return new Dictionary<Guid, SchoolReference>();

        return await _schools.TableNoTracking
            .Where(s => ids.Contains(s.Id))
            .Select(s => new SchoolReference(s.Id, s.Code, s.Name, s.Status == SchoolStatus.Active))
            .ToDictionaryAsync(s => s.Id, cancellationToken);
    }

    public async Task<string?> ClassOfAsync(Guid schoolId, string? text, CancellationToken cancellationToken = default)
    {
        var key = SchoolClass.KeyOf(text);
        if (key is null)
            return null;

        return await _schools.TableNoTracking
            .Where(s => s.Id == schoolId)
            .SelectMany(s => s.Classes)
            .Where(c => !c.IsDeleted && c.NameNormalized == key)
            .Select(c => c.Name)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
