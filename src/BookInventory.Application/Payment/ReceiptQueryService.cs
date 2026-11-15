using BookInventory.Application.Sales;
using BookInventory.Application.School;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.BuildingBlocks.Domain.Models;
using BookInventory.Domain.Payment;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Application.Payment;

/// <summary>R16: the receipt list and one receipt with its modes, allocations and advance. The school and the invoice numbers come from their directories.</summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class ReceiptQueryService(IReceiptStore store, ISchoolDirectory schools, IInvoiceDirectory invoices)
{
    public async Task<IPagedList<ReceiptSummary>> ListAsync(ReceiptQuery query, CancellationToken cancellationToken)
    {
        var page = await store.SearchAsync(query, cancellationToken);
        var references = await schools.ReferencesAsync([.. page.Items.Select(r => r.SchoolId).Distinct()], cancellationToken);
        return new PagedList<ReceiptSummary>
        {
            PageIndex = page.PageIndex,
            PageSize = page.PageSize,
            IndexFrom = page.IndexFrom,
            TotalCount = page.TotalCount,
            TotalPages = page.TotalPages,
            Items =
            [
                .. page.Items.Select(r => new ReceiptSummary(
                    r.Id, r.Number, r.ReceiptDate, r.SchoolId,
                    references.GetValueOrDefault(r.SchoolId)?.Code, references.GetValueOrDefault(r.SchoolId)?.Name, r.TotalAmount, r.Status))
            ]
        };
    }

    public async Task<ReceiptDetail> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var receipt = await store.FindForReadAsync(id, cancellationToken) ?? throw new NotFoundException(PaymentMessages.ReceiptNotFound);
        var school = await schools.GetAsync(receipt.SchoolId, cancellationToken);
        var numbers = (await invoices.ReferencesAsync([.. receipt.Allocations.Select(a => a.InvoiceId).Distinct()], cancellationToken))
            .ToDictionary(p => p.Key, p => p.Value.Number);
        return ReceiptResponses.Detail(receipt, school, numbers) with { Proofs = await store.ProofsOfAsync(id, cancellationToken) };
    }
}
