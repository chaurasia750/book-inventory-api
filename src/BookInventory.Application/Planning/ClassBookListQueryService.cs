using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.Domain.Planning;
using BookInventory.Domain.School;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Application.Planning;

/// <summary>
/// Spec planning R7: a school's lists of one session, every class of the school (and any list of a class the school no longer
/// has), each compared with the same class of the session before.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class ClassBookListQueryService(IClassBookListStore store, BookListScope scope)
{
    public async Task<SchoolBookListsResponse> SessionListsAsync(Guid schoolId, string? session, CancellationToken cancellationToken)
    {
        var label = string.IsNullOrWhiteSpace(session)
            ? await scope.CurrentSessionAsync(schoolId, cancellationToken)
            : await scope.SessionAsync(schoolId, session, cancellationToken);
        var previous = SchoolSession.PreviousOf(label)!;
        var lists = ByClass(await store.ListsOfAsync(schoolId, label, false, cancellationToken));
        var before = ByClass(await store.ListsOfAsync(schoolId, previous, false, cancellationToken));

        var classes = (await scope.ClassesAsync(schoolId, cancellationToken)).ToList();
        var known = classes.Select(c => SchoolClass.KeyOf(c)!).ToHashSet();
        classes.AddRange(lists.Where(l => !known.Contains(l.Key)).Select(l => l.Value.ClassName));

        var rows = classes.Select(c =>
        {
            var key = SchoolClass.KeyOf(c)!;
            return ClassOf(c, lists.GetValueOrDefault(key), before.GetValueOrDefault(key));
        }).ToList();
        return new SchoolBookListsResponse(schoolId, label, previous, rows);
    }

    private static Dictionary<string, ClassBookList> ByClass(IReadOnlyList<ClassBookList> lists) =>
        lists.ToDictionary(l => SchoolClass.KeyOf(l.ClassName)!);

    private static ClassBookListResponse ClassOf(string className, ClassBookList? list, ClassBookList? previous)
    {
        var before = previous?.Lines.ToDictionary(l => l.ItemId) ?? [];
        var lines = (list?.Lines ?? []).Select(l =>
        {
            var old = before.GetValueOrDefault(l.ItemId);
            var change = previous is null ? BookListChange.Same
                : old is null ? BookListChange.Added
                : old.QuantityPerStudent != l.QuantityPerStudent ? BookListChange.Changed
                : BookListChange.Same;
            return new BookListLineResponse(l.ItemId, l.ItemCode, l.ItemName, l.QuantityPerStudent, change, change == BookListChange.Changed ? old!.QuantityPerStudent : null);
        }).ToList();

        var current = list?.Lines.Select(l => l.ItemId).ToHashSet() ?? [];
        var removed = list is null ? [] : before.Values.Where(l => !current.Contains(l.ItemId))
            .Select(l => new RemovedBookResponse(l.ItemId, l.ItemCode, l.ItemName, l.QuantityPerStudent)).ToList();
        return new ClassBookListResponse(list?.ClassName ?? className, list is not null, list?.StudentEstimate, lines, removed);
    }
}
