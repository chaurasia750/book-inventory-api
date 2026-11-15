using BookInventory.Application.Sales;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Persistence.Contracts;
using BookInventory.Domain.Sales;
using Microsoft.EntityFrameworkCore;

namespace BookInventory.Infrastructure.Sales;

/// <summary>Spec sales R26: a school's invoice layout through the company-filtered repository.</summary>
[AutoRegister(ServiceLifetime.Scoped, typeof(IInvoiceLayoutStore))]
public sealed class InvoiceLayoutStore(IUnitOfWork unitOfWork) : IInvoiceLayoutStore
{
    private readonly IRepository<SchoolInvoiceLayout> _layouts = unitOfWork.Repository<SchoolInvoiceLayout>();

    public Task AddAsync(SchoolInvoiceLayout layout, CancellationToken cancellationToken = default) => _layouts.AddAsync(layout, cancellationToken);

    public Task<SchoolInvoiceLayout?> FindForManagementAsync(Guid schoolId, CancellationToken cancellationToken = default) =>
        _layouts.Table.FirstOrDefaultAsync(l => l.SchoolId == schoolId, cancellationToken);

    public Task<SchoolInvoiceLayout?> FindForReadAsync(Guid schoolId, CancellationToken cancellationToken = default) =>
        _layouts.TableNoTracking.FirstOrDefaultAsync(l => l.SchoolId == schoolId, cancellationToken);
}
