using BookInventory.Application.School;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.Domain.Sales;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Application.Sales;

/// <summary>R41: suggests the school's students while a name is typed (at least two letters, at most 20, optionally one class).</summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class StudentQueryService(ICounterStore store, ISchoolDirectory schools)
{
    public async Task<IReadOnlyList<StudentSuggestion>> SuggestAsync(StudentSuggestQuery query, CancellationToken cancellationToken)
    {
        var text = Student.NormalizeName(query.Text);
        if (text.Length < 2)
            return [];

        var schoolId = query.SchoolId!.Value;
        if (await schools.GetAsync(schoolId, cancellationToken) is null)
            throw new NotFoundException(CounterMessages.SchoolNotFound);

        var className = string.IsNullOrWhiteSpace(query.ClassName) ? null : await schools.ClassOfAsync(schoolId, query.ClassName, cancellationToken);
        if (!string.IsNullOrWhiteSpace(query.ClassName) && className is null)
            return [];

        var students = await store.SuggestStudentsAsync(schoolId, text, className, cancellationToken);
        return [.. students.Select(s => new StudentSuggestion(s.Id, s.Name, s.ClassName, s.Section, s.RollNumber))];
    }
}
