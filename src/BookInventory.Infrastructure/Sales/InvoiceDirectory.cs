using BookInventory.Application.Sales;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Persistence.Contracts;
using BookInventory.Domain.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Infrastructure.Sales;

/// <summary>
/// Spec sales R37: the read capability other modules use, over the same company-filtered repository as the sales store, so it only
/// ever sees the session's company. Nothing is tracked and nothing is changed.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped, typeof(IInvoiceDirectory))]
public sealed class InvoiceDirectory(IUnitOfWork unitOfWork) : IInvoiceDirectory
{
    private readonly IRepository<Invoice> _invoices = unitOfWork.Repository<Invoice>();

    public async Task<IReadOnlyList<InvoiceReference>> IssuedOfSchoolAsync(Guid schoolId, CancellationToken cancellationToken = default) =>
        await ReferencesOf(_invoices.TableNoTracking
                .Where(i => i.SchoolId == schoolId && i.Status == InvoiceStatus.Issued)
                .OrderBy(i => i.InvoiceDate).ThenBy(i => i.Number))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyDictionary<Guid, InvoiceReference>> ReferencesAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default)
    {
        if (ids.Count == 0)
            return new Dictionary<Guid, InvoiceReference>();

        return await ReferencesOf(_invoices.TableNoTracking.Where(i => ids.Contains(i.Id))).ToDictionaryAsync(i => i.Id, cancellationToken);
    }

    private static IQueryable<InvoiceReference> ReferencesOf(IQueryable<Invoice> invoices) =>
        invoices.Select(i => new InvoiceReference(i.Id, i.Number, i.InvoiceDate, i.Session, i.SchoolId, i.Totals.GrandTotal, i.Status));
}
