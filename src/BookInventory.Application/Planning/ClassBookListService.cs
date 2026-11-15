using BookInventory.Application.Item;
using BookInventory.Application.Price;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.Domain.Planning;
using BookInventory.Domain.School;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Application.Planning;

/// <summary>
/// Spec planning R2-R6: saves, deletes and copies class book lists. It checks what the request names (<see cref="BookListScope"/>,
/// the item directory) and lets the list keep its own rules.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class ClassBookListService(IClassBookListStore store, BookListScope scope, IItemDirectory items, PriceCompanyGuard companyGuard)
{
    /// <summary>R2, R3, R5: creates or replaces one class's list.</summary>
    public async Task SaveAsync(BookListKey key, SaveBookListRequest request, CancellationToken cancellationToken)
    {
        companyGuard.Require();
        var session = await scope.SessionAsync(key.SchoolId, key.Session, cancellationToken);
        var className = await scope.ClassAsync(key.SchoolId, key.ClassName, cancellationToken);
        var list = await store.FindForManagementAsync(key.SchoolId, session, className, cancellationToken);
        var lines = await LinesAsync(request.Lines ?? [], list, cancellationToken);
        if (list is null)
        {
            list = ClassBookList.Start(key.SchoolId, session, className);
            await store.AddAsync(list, cancellationToken);
        }

        list.Replace(request.StudentEstimate, lines);
    }

    /// <summary>R5: removes one class's list (soft).</summary>
    public async Task DeleteAsync(BookListKey key, CancellationToken cancellationToken)
    {
        companyGuard.Require();
        var session = await scope.SessionAsync(key.SchoolId, key.Session, cancellationToken);
        var className = await scope.ClassAsync(key.SchoolId, key.ClassName, cancellationToken);
        var list = await store.FindForRemovalAsync(key.SchoolId, session, className, cancellationToken)
                   ?? throw new NotFoundException(PlanningMessages.ListNotFound);
        store.Remove(list);
    }

    /// <summary>R4: copies every list of the source session into this school's session, class by class; estimates are not copied.</summary>
    public async Task<CopyBookListsResponse> CopyAsync(Guid schoolId, string? session, CopyBookListsRequest request, CancellationToken cancellationToken)
    {
        companyGuard.Require();
        var target = await scope.SessionAsync(schoolId, session, cancellationToken);
        var fromSchool = request.FromSchoolId ?? schoolId;
        var fromSession = await scope.SessionAsync(fromSchool, request.FromSession, cancellationToken);
        if (fromSchool == schoolId && fromSession == target)
            throw new ValidationFailureException(PlanningMessages.SameSource);

        var sources = await store.ListsOfAsync(fromSchool, fromSession, false, cancellationToken);
        var existing = (await store.ListsOfAsync(schoolId, target, true, cancellationToken)).ToDictionary(l => SchoolClass.KeyOf(l.ClassName)!);
        var classes = (await scope.ClassesAsync(schoolId, cancellationToken)).ToDictionary(c => SchoolClass.KeyOf(c)!);
        var copied = new List<string>();
        var skipped = new List<string>();
        foreach (var source in sources)
        {
            var classKey = SchoolClass.KeyOf(source.ClassName)!;
            var current = existing.GetValueOrDefault(classKey);
            if (!classes.TryGetValue(classKey, out var className) || (current is not null && !request.ReplaceExisting))
            {
                skipped.Add(source.ClassName);
                continue;
            }

            (current ?? await StartAsync(new BookListKey(schoolId, target, className), cancellationToken)).CopyBooksFrom(source);
            copied.Add(className);
        }

        return new CopyBookListsResponse(copied, skipped);
    }

    private async Task<ClassBookList> StartAsync(BookListKey key, CancellationToken cancellationToken)
    {
        var list = ClassBookList.Start(key.SchoolId, key.Session, key.ClassName);
        await store.AddAsync(list, cancellationToken);
        return list;
    }

    /// <summary>R2: every item must exist; an item that is not on the list yet must be active.</summary>
    private async Task<IReadOnlyList<BookListLineInput>> LinesAsync(IReadOnlyList<BookListLineRequest> requested, ClassBookList? list, CancellationToken cancellationToken)
    {
        var ids = requested.Select(l => l.ItemId!.Value).Distinct().ToList();
        var references = await items.ReferencesAsync(ids, cancellationToken);
        var onList = list?.Lines.Select(l => l.ItemId).ToHashSet() ?? [];
        return requested.Select(l =>
        {
            var item = references.GetValueOrDefault(l.ItemId!.Value) ?? throw new ValidationFailureException(PlanningMessages.ItemNotFound);
            if (!item.IsActive && !onList.Contains(item.Id))
                throw new ValidationFailureException(PlanningMessages.ItemInactive);

            return new BookListLineInput(item.Id, item.Code, item.Name, l.QuantityPerStudent!.Value);
        }).ToList();
    }
}
