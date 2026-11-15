using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.Domain.School;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Application.Planning;

/// <summary>Spec planning R2, R3: the read capability <see cref="IClassBookLists"/> over the company-filtered store.</summary>
[AutoRegister(ServiceLifetime.Scoped, typeof(IClassBookLists))]
public sealed class ClassBookLists(IClassBookListStore store) : IClassBookLists
{
    public async Task<IReadOnlyDictionary<string, ClassBookListReference>> ListsOfAsync(Guid schoolId, string session, CancellationToken cancellationToken = default)
    {
        var lists = await store.ListsOfAsync(schoolId, session, false, cancellationToken);
        return lists.ToDictionary(
            l => SchoolClass.KeyOf(l.ClassName)!,
            l => new ClassBookListReference(l.ClassName, l.StudentEstimate, l.Lines.Select(b => new ClassBookReference(b.ItemId, b.ItemCode, b.QuantityPerStudent)).ToList()));
    }
}
