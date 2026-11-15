using BookInventory.Application.Payment;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Models;
using BookInventory.BuildingBlocks.Persistence.Contracts;
using BookInventory.BuildingBlocks.Persistence.Extensions;
using BookInventory.Domain.Payment;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Infrastructure.Payment;

/// <summary>The data side of the payment use cases (spec payment R3, R9, R16); every query is limited to the session's company.</summary>
[AutoRegister(ServiceLifetime.Scoped, typeof(IReceiptStore))]
public sealed class ReceiptStore(IUnitOfWork unitOfWork) : IReceiptStore
{
    private const int MaxPageSize = 200;

    private readonly IRepository<Receipt> _receipts = unitOfWork.Repository<Receipt>();

    public Task AddAsync(Receipt receipt, CancellationToken cancellationToken = default) => _receipts.AddAsync(receipt, cancellationToken);

    public async Task<Guid?> FindIdByClientRequestAsync(Guid clientRequestId, CancellationToken cancellationToken = default) =>
        await _receipts.TableNoTracking.Where(r => r.ClientRequestId == clientRequestId).Select(r => (Guid?)r.Id).FirstOrDefaultAsync(cancellationToken);

    public Task<Receipt?> FindForManagementAsync(Guid id, CancellationToken cancellationToken = default) =>
        _receipts.Table.Include(r => r.Modes).Include(r => r.Allocations).FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public async Task<int> CountProofsAsync(Guid receiptId, CancellationToken cancellationToken = default) =>
        await _receipts.TableNoTracking.Where(r => r.Id == receiptId).SelectMany(r => r.Proofs).CountAsync(cancellationToken);

    public async Task<IReadOnlyList<ReceiptProofDto>> ProofsOfAsync(Guid receiptId, CancellationToken cancellationToken = default) =>
        await _receipts.TableNoTracking.Where(r => r.Id == receiptId).SelectMany(r => r.Proofs)
            .OrderBy(p => p.Id)
            .Select(p => new ReceiptProofDto(p.Id, p.FileName, p.ContentType, p.SizeBytes))
            .ToListAsync(cancellationToken);

    public async Task<ReceiptProofFile?> FindProofAsync(Guid receiptId, Guid proofId, CancellationToken cancellationToken = default) =>
        await _receipts.TableNoTracking.Where(r => r.Id == receiptId).SelectMany(r => r.Proofs).Where(p => p.Id == proofId)
            .Select(p => new ReceiptProofFile(p.FileName, p.ContentType, p.Content))
            .FirstOrDefaultAsync(cancellationToken);

    public Task<Receipt?> FindForProofRemovalAsync(Guid receiptId, Guid proofId, CancellationToken cancellationToken = default) =>
        _receipts.Table.Include(r => r.Proofs.Where(p => p.Id == proofId)).FirstOrDefaultAsync(r => r.Id == receiptId, cancellationToken);

    public async Task<IReadOnlyList<Receipt>> FindAppliedToInvoiceAsync(Guid invoiceId, CancellationToken cancellationToken = default) =>
        await _receipts.Table.Include(r => r.Modes).Include(r => r.Allocations)
            .Where(r => r.Allocations.Any(a => a.InvoiceId == invoiceId))
            .ToListAsync(cancellationToken);

    public Task<Receipt?> FindForReadAsync(Guid id, CancellationToken cancellationToken = default) =>
        _receipts.TableNoTracking.Include(r => r.Modes).Include(r => r.Allocations).FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public async Task<IPagedList<ReceiptRow>> SearchAsync(ReceiptQuery query, CancellationToken cancellationToken = default)
    {
        var search = query.Search?.Trim().ToLowerInvariant();
        var rows = _receipts.TableNoTracking
            .Where(r => query.SchoolId == null || r.SchoolId == query.SchoolId)
            .Where(r => query.Status == null || r.Status == query.Status)
            .Where(r => query.From == null || r.ReceiptDate >= query.From)
            .Where(r => query.To == null || r.ReceiptDate <= query.To)
            .Where(r => string.IsNullOrEmpty(search) || r.Number.ToLower().Contains(search))
            .Where(r => query.Mode == null || r.Modes.Any(m => m.ModeType == query.Mode))
            .Where(r => query.ChequeStatus == null || r.Modes.Any(m => m.ChequeStatus == query.ChequeStatus))
            .OrderByDescending(r => r.ReceiptDate).ThenByDescending(r => r.CreatedAt)
            .Select(r => new ReceiptRow(r.Id, r.Number, r.ReceiptDate, r.SchoolId, r.TotalAmount, r.Status));
        return await rows.ToPagedListAsync(query.PageIndex < 1 ? 1 : query.PageIndex, Math.Clamp(query.PageSize < 1 ? 20 : query.PageSize, 1, MaxPageSize), cancellationToken);
    }

    public async Task<IReadOnlyDictionary<Guid, decimal>> AllocatedAsync(IReadOnlyCollection<Guid> invoiceIds, CancellationToken cancellationToken = default)
    {
        if (invoiceIds.Count == 0)
            return new Dictionary<Guid, decimal>();

        return await _receipts.TableNoTracking
            .SelectMany(r => r.Allocations)
            .Where(a => invoiceIds.Contains(a.InvoiceId))
            .GroupBy(a => a.InvoiceId)
            .Select(g => new { InvoiceId = g.Key, Total = g.Sum(a => a.Amount) })
            .ToDictionaryAsync(x => x.InvoiceId, x => x.Total, cancellationToken);
    }
}
