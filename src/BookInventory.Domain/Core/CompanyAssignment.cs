using BookInventory.BuildingBlocks.Domain.Exceptions;

namespace BookInventory.Domain.Core;

/// <summary>
/// Spec platform P20: the one rule of assigning a company to a row, shared by <see cref="CompanyOwnedRoot"/> and
/// <see cref="CompanyOwnedChild"/>: an empty id is refused, the first company is accepted, the same company again is
/// harmless, and a different company is refused.
/// </summary>
internal static class CompanyAssignment
{
    public static Guid Apply(Guid current, Guid companyId)
    {
        if (companyId == Guid.Empty)
            throw new ForbiddenException(CoreMessages.CompanyRequired);
        if (current != Guid.Empty && current != companyId)
            throw new ValidationFailureException(CoreMessages.CompanyImmutable);

        return companyId;
    }
}
