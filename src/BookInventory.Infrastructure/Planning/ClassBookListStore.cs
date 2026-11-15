using BookInventory.Application.Planning;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Persistence.Contracts;
using BookInventory.Domain.Planning;
using Microsoft.EntityFrameworkCore;

namespace BookInventory.Infrastructure.Planning;

/// <summary>Spec planning R1: class book lists through the company-filtered repository (another company's list is never seen).</summary>
[AutoRegister(ServiceLifetime.Scoped, typeof(IClassBookListStore))]
public sealed class ClassBookListStore(IUnitOfWork unitOfWork) : IClassBookListStore
{
    private readonly IRepository<ClassBookList> _lists = unitOfWork.Repository<ClassBookList>();

    public Task AddAsync(ClassBookList list, CancellationToken cancellationToken = default) => _lists.AddAsync(list, cancellationToken);

    public void Remove(ClassBookList list) => _lists.Remove(list);

    public Task<ClassBookList?> FindForManagementAsync(Guid schoolId, string session, string className, CancellationToken cancellationToken = default) =>
        _lists.Table.Include(l => l.Lines)
            .FirstOrDefaultAsync(l => l.SchoolId == schoolId && l.Session == session && l.ClassName == className, cancellationToken);

    public Task<ClassBookList?> FindForRemovalAsync(Guid schoolId, string session, string className, CancellationToken cancellationToken = default) =>
        _lists.Table.FirstOrDefaultAsync(l => l.SchoolId == schoolId && l.Session == session && l.ClassName == className, cancellationToken);

    public async Task<IReadOnlyList<ClassBookList>> ListsOfAsync(Guid schoolId, string session, bool tracked, CancellationToken cancellationToken = default) =>
        await (tracked ? _lists.Table : _lists.TableNoTracking).Include(l => l.Lines)
            .Where(l => l.SchoolId == schoolId && l.Session == session)
            .ToListAsync(cancellationToken);
}
