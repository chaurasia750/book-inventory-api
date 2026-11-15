using BookInventory.Application.School;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.Domain.Sales;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Application.Sales;

/// <summary>
/// R40, R41: the students of the school counter sale. A bill gives a student by id or by name and class; the student of that name
/// (ignoring case and extra spaces) and class is reused with the values the bill knows, otherwise one is created. It runs inside
/// the bill's own request transaction, so a bill that fails creates no student. The class is checked and spelled through the
/// school directory (school R16).
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class StudentRecordService(ICounterStore store, ISchoolDirectory schools)
{
    /// <summary>R41: the student for a bill: the one named by id (it must belong to the school) or the one found or created from the input.</summary>
    public async Task<Student> ForBillAsync(Guid schoolId, Guid? studentId, StudentInput? input, CancellationToken cancellationToken)
    {
        if (studentId is { } id)
        {
            var known = await store.FindStudentByIdAsync(id, cancellationToken);
            return known is not null && known.SchoolId == schoolId ? known : throw new NotFoundException(CounterMessages.StudentNotFound);
        }

        if (input is null)
            throw new ValidationFailureException(CounterMessages.InvalidStudentName);

        var className = await schools.ClassOfAsync(schoolId, input.ClassName, cancellationToken)
            ?? throw new ValidationFailureException(CounterMessages.InvalidStudentClass);
        var details = input.ToDetails(className);
        var existing = await store.FindStudentAsync(schoolId, Student.NormalizeName(input.Name), className, cancellationToken);
        if (existing is not null)
        {
            existing.Learn(details);
            return existing;
        }

        var student = Student.Create(schoolId, details);
        await store.AddStudentAsync(student, cancellationToken);
        return student;
    }
}
