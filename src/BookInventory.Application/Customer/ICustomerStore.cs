using BookInventory.BuildingBlocks.Domain.Models;
using CustomerRoot = BookInventory.Domain.Customer.Customer;

namespace BookInventory.Application.Customer;

/// <summary>One list row as the store reads it; the school's name is added by the query service from the school directory.</summary>
public sealed record CustomerRow(
    Guid Id,
    string Code,
    string Name,
    string Mobile,
    Guid? SchoolId,
    string? ClassName,
    Domain.Customer.CustomerStatus Status,
    DateTime CreatedAt);

/// <summary>
/// The persistence port of the <c>customer</c> module. Every read is already limited to the session's company by the platform
/// filter (P19), so nothing here takes a company. Like the other stores it has no save method: the request transaction commits.
/// </summary>
public interface ICustomerStore
{
    /// <summary>The cap of <see cref="SuggestAsync"/> (R8).</summary>
    const int MaxSuggestions = 20;

    Task AddAsync(CustomerRoot customer, CancellationToken cancellationToken = default);

    /// <summary>The customer about to be changed: tracked. <c>null</c> for an unknown id or another company's.</summary>
    Task<CustomerRoot?> FindForManagementAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>The customer for reading: not tracked.</summary>
    Task<CustomerRoot?> FindForReadAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>R3: whether another customer of the company already has this stored mobile number.</summary>
    Task<bool> IsMobileTakenAsync(string mobile, Guid? excludingId, CancellationToken cancellationToken = default);

    /// <summary>R11: the paged, searchable, filterable list.</summary>
    Task<IPagedList<CustomerRow>> SearchAsync(CustomerSearchQuery query, CancellationToken cancellationToken = default);

    /// <summary>R8: at most <see cref="MaxSuggestions"/> active customers whose name or mobile contains the text.</summary>
    Task<IReadOnlyList<CustomerRow>> SuggestAsync(string text, CancellationToken cancellationToken = default);
}
