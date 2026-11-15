using BookInventory.Domain.Sales;

namespace BookInventory.Application.Sales;

/// <summary>Spec sales R26: the persistence port of a school's invoice layout (company-filtered).</summary>
public interface IInvoiceLayoutStore
{
    Task AddAsync(SchoolInvoiceLayout layout, CancellationToken cancellationToken = default);

    Task<SchoolInvoiceLayout?> FindForManagementAsync(Guid schoolId, CancellationToken cancellationToken = default);

    Task<SchoolInvoiceLayout?> FindForReadAsync(Guid schoolId, CancellationToken cancellationToken = default);
}
