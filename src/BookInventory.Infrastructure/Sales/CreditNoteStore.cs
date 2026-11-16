using BookInventory.Application.Sales;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Models;
using BookInventory.BuildingBlocks.Persistence.Contracts;
using BookInventory.BuildingBlocks.Persistence.Extensions;
using BookInventory.Domain.Sales;
using Microsoft.EntityFrameworkCore;

namespace BookInventory.Infrastructure.Sales;

/// <summary>The data side of the credit notes (spec sales R53-R63). Every query is already limited to the session's company by the platform filter.</summary>
[AutoRegister(ServiceLifetime.Scoped, typeof(ICreditNoteStore))]
public sealed class CreditNoteStore(IUnitOfWork unitOfWork) : ICreditNoteStore
{
    private static readonly string[] SortableColumns = ["NoteDate", "Number", "CreatedAt"];

    private readonly IRepository<CreditNote> _notes = unitOfWork.Repository<CreditNote>();
    private readonly IRepository<Invoice> _invoices = unitOfWork.Repository<Invoice>();

    public Task AddAsync(CreditNote note, CancellationToken cancellationToken = default) => _notes.AddAsync(note, cancellationToken);

    public Task<CreditNote?> FindForManagementAsync(Guid id, CancellationToken cancellationToken = default) =>
        _notes.Table.Include(n => n.Lines).FirstOrDefaultAsync(n => n.Id == id, cancellationToken);

    public Task<CreditNote?> FindForReadAsync(Guid id, CancellationToken cancellationToken = default) =>
        _notes.TableNoTracking.Include(n => n.Lines).FirstOrDefaultAsync(n => n.Id == id, cancellationToken);

    public async Task<IPagedList<CreditNote>> SearchAsync(CreditNoteSearchQuery query, CancellationToken cancellationToken = default)
    {
        var search = query.Search?.Trim().ToLowerInvariant();
        var session = query.Session?.Trim();
        var (schoolId, invoiceId, status, reason, from, to) = (query.SchoolId, query.InvoiceId, query.Status, query.Reason, query.From, query.To);

        return await _notes.TableNoTracking
            .Where(n => schoolId == null || n.SchoolId == schoolId)
            .Where(n => invoiceId == null || n.InvoiceId == invoiceId)
            .Where(n => status == null || n.Status == status)
            .Where(n => reason == null || n.Reason == reason)
            .Where(n => string.IsNullOrEmpty(session) || n.Session == session)
            .Where(n => from == null || n.NoteDate >= from)
            .Where(n => to == null || n.NoteDate <= to)
            .Where(n => string.IsNullOrEmpty(search) || n.Number.ToLower().Contains(search) || n.SchoolName.ToLower().Contains(search) || n.SchoolCode.ToLower().Contains(search))
            .OrderBy(WhitelistedOrder(query.OrderBy))
            .ToPagedListAsync(query.PageIndex < 1 ? 1 : query.PageIndex, query.PageSize, cancellationToken);
    }

    public async Task<IReadOnlyDictionary<Guid, int>> ReturnedQuantitiesAsync(Guid invoiceId, CancellationToken cancellationToken = default) =>
        await _notes.TableNoTracking
            .Where(n => n.InvoiceId == invoiceId && n.Status == CreditNoteStatus.Active)
            .SelectMany(n => n.Lines)
            .Where(l => l.InvoiceLineId != null)
            .GroupBy(l => l.InvoiceLineId!.Value)
            .Select(g => new { Line = g.Key, Quantity = g.Sum(l => l.Quantity) })
            .ToDictionaryAsync(x => x.Line, x => x.Quantity, cancellationToken);

    public async Task<IReadOnlyDictionary<Guid, LastInvoicedPrice>> LastInvoicedPricesAsync(
        Guid schoolId, IReadOnlyCollection<Guid> itemIds, CancellationToken cancellationToken = default)
    {
        var rows = await _invoices.TableNoTracking
            .Where(i => i.SchoolId == schoolId && i.Status == InvoiceStatus.Issued)
            .SelectMany(i => i.Lines.Select(l => new { l.ItemId, l.PriceVersionId, l.PriceVersionName, l.MrpAmount, l.RateAmount, i.InvoiceDate, i.CreatedAt }))
            .Where(x => itemIds.Contains(x.ItemId))
            .OrderByDescending(x => x.InvoiceDate).ThenByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
        return rows.GroupBy(x => x.ItemId).ToDictionary(
            g => g.Key, g => new LastInvoicedPrice(g.Key, g.First().PriceVersionId, g.First().PriceVersionName, g.First().MrpAmount, g.First().RateAmount));
    }

    public async Task<IReadOnlyList<CreditNote>> OfInvoiceAsync(Guid invoiceId, CancellationToken cancellationToken = default) =>
        await _notes.TableNoTracking.Where(n => n.InvoiceId == invoiceId).OrderByDescending(n => n.NoteDate).ThenByDescending(n => n.CreatedAt).ToListAsync(cancellationToken);

    public Task<bool> HasLiveForInvoiceAsync(Guid invoiceId, CancellationToken cancellationToken = default) =>
        _notes.TableNoTracking.AnyAsync(n => n.InvoiceId == invoiceId && n.Status == CreditNoteStatus.Active, cancellationToken);

    /// <summary>The platform <c>OrderBy(string)</c> accepts any property, so the column is checked here first; anything else falls back to the default order.</summary>
    private static string? WhitelistedOrder(string? orderBy)
    {
        var parts = (orderBy ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var column = SortableColumns.FirstOrDefault(c => parts.Length > 0 && c.Equals(parts[0], StringComparison.OrdinalIgnoreCase));
        if (column is null)
            return null;

        var descending = parts.Length > 1 && parts[1].Equals("DESC", StringComparison.OrdinalIgnoreCase);
        return $"{column} {(descending ? "DESC" : "ASC")}";
    }
}
