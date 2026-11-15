using BookInventory.Application.School;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.Domain.Customer;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Application.Customer;

/// <summary>
/// R4: the school of a customer must be an active school of the company and its class one of that school's classes, both read
/// through the school directory (school R16). A school the customer already has may stay even when it was deactivated later;
/// only a newly chosen one must be active. The class is answered in the spelling the school writes it.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class CustomerSchoolGuard(ISchoolDirectory schools)
{
    public async Task<string?> ResolveClassAsync(
        Guid? schoolId, string? className, Guid? currentSchoolId, CancellationToken cancellationToken)
    {
        if (schoolId is null)
            return className;

        await RequireUsableAsync(schoolId.Value, currentSchoolId, cancellationToken);
        if (string.IsNullOrWhiteSpace(className))
            return null;

        return await schools.ClassOfAsync(schoolId.Value, className, cancellationToken)
            ?? throw new ValidationFailureException(CustomerMessages.InvalidClass);
    }

    private async Task RequireUsableAsync(Guid schoolId, Guid? currentSchoolId, CancellationToken cancellationToken)
    {
        var school = await schools.GetAsync(schoolId, cancellationToken);
        if (school is null || (!school.IsActive && schoolId != currentSchoolId))
            throw new ValidationFailureException(CustomerMessages.InvalidSchool);
    }
}
