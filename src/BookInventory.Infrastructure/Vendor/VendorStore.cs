using BookInventory.Application.Vendor;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Models;
using BookInventory.BuildingBlocks.Persistence.Contracts;
using BookInventory.BuildingBlocks.Persistence.Extensions;
using BookInventory.Domain.Vendor;
using Microsoft.EntityFrameworkCore;
using VendorRoot = BookInventory.Domain.Vendor.Vendor;
using BookInventory.Domain.Core;

namespace BookInventory.Infrastructure.Vendor;

/// <summary>
/// The data side of the vendor use cases (spec vendor R4, R10). Every query is already limited to the session's company by
/// the platform filter. The list carries the account number only in its masked form (R5).
/// </summary>
[AutoRegister(ServiceLifetime.Scoped, typeof(IVendorStore))]
public sealed class VendorStore(IUnitOfWork unitOfWork) : IVendorStore
{
    private static readonly string[] SortableColumns = ["Name", "Code", "CreatedAt"];

    private readonly IRepository<VendorRoot> _vendors = unitOfWork.Repository<VendorRoot>();

    public Task AddAsync(VendorRoot vendor, CancellationToken cancellationToken = default) =>
        _vendors.AddAsync(vendor, cancellationToken);

    public Task<VendorRoot?> FindForManagementAsync(Guid id, CancellationToken cancellationToken = default) =>
        _vendors.Table.FirstOrDefaultAsync(v => v.Id == id, cancellationToken);

    public Task<VendorRoot?> FindForReadAsync(Guid id, CancellationToken cancellationToken = default) =>
        _vendors.TableNoTracking.FirstOrDefaultAsync(v => v.Id == id, cancellationToken);

    public Task<bool> IsNameTakenAsync(string normalizedName, Guid? excludingId, CancellationToken cancellationToken = default) =>
        _vendors.TableNoTracking.AnyAsync(
            v => v.NameNormalized == normalizedName && (excludingId == null || v.Id != excludingId), cancellationToken);

    public async Task<IPagedList<VendorSummary>> SearchAsync(VendorSearchQuery query, CancellationToken cancellationToken = default)
    {
        var search = query.Search?.Trim().ToLowerInvariant();
        var status = query.Status;
        var state = query.State?.Trim();

        var page = await _vendors.TableNoTracking
            .Where(v => status == null || v.Status == status)
            .Where(v => string.IsNullOrEmpty(state) || v.State == state)
            .Where(v => string.IsNullOrEmpty(search)
                        || v.Name.ToLower().Contains(search)
                        || v.Code.ToLower().Contains(search)
                        || (v.ContactPerson != null && v.ContactPerson.ToLower().Contains(search))
                        || (v.Mobile != null && v.Mobile.Contains(search))
                        || (v.Gstin != null && v.Gstin.ToLower().Contains(search)))
            .OrderBy(WhitelistedOrder(query.OrderBy))
            .Select(v => new VendorRow(v.Id, v.Code, v.Name, v.ContactPerson, v.Mobile, v.State, v.BankAccountNumber, v.Status, v.CreatedAt))
            .ToPagedListAsync(query.PageIndex < 1 ? 1 : query.PageIndex, query.PageSize, cancellationToken);

        return new PagedList<VendorSummary>
        {
            PageIndex = page.PageIndex,
            PageSize = page.PageSize,
            IndexFrom = page.IndexFrom,
            TotalCount = page.TotalCount,
            TotalPages = page.TotalPages,
            Items =
            [
                .. page.Items.Select(r => new VendorSummary(
                    r.Id, r.Code, r.Name, r.ContactPerson, r.Mobile, r.State, BankAccount.Mask(r.BankAccountNumber), r.Status, r.CreatedAt))
            ]
        };
    }

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

    /// <summary>The list row as read from the database: the number is masked right after, before it leaves the store.</summary>
    private sealed record VendorRow(
        Guid Id, string Code, string Name, string? ContactPerson, string? Mobile, string? State, string? BankAccountNumber,
        VendorStatus Status, DateTime CreatedAt);
}
