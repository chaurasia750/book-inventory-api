using System.ComponentModel.DataAnnotations;
using BookInventory.Domain.Purchase;

namespace BookInventory.Application.Purchase;

/// <summary>R4: one returned line: the item, the quantity, where it leaves from (Main or Damaged) and optionally a changed rate (empty: the item's purchase price).</summary>
public sealed record VendorReturnLineRequest(
    [param: Required] Guid? ItemId,
    [param: Required, Range(1, 100_000)] int? Quantity,
    [param: Required] ReturnStockSource? StockSource,
    decimal? Rate = null);

/// <summary>R2-R5: a new vendor return: the vendor, the lines, the optional date and remark.</summary>
public sealed record MakeVendorReturnRequest(
    [param: Required] Guid? VendorId,
    IReadOnlyList<VendorReturnLineRequest> Lines,
    DateOnly? ReturnDate = null,
    [param: StringLength(VendorReturn.RemarkMaxLength)] string? Remark = null);

/// <summary>R8: the Owner's reason (5-500 characters).</summary>
public sealed record CancelVendorReturnRequest([param: Required] string? Reason);

/// <summary>R6: the vendor return made, its total and the stock items whose stock went to zero or below (never a refusal).</summary>
public sealed record VendorReturnMade(Guid VendorReturnId, string Number, decimal GrandTotal, IReadOnlyList<Guid> ItemsAtOrBelowZero);

/// <summary>R3: the number, the date and today a vendor return is stamped with.</summary>
public sealed record VendorReturnStamp(string Number, DateOnly ReturnDate, DateOnly Today);

/// <summary>R12: the vendor return list filters and paging. <see cref="OrderBy"/> is whitelisted column names only.</summary>
public sealed record VendorReturnSearchQuery
{
    /// <summary>Free text over the number and the vendor's name or code.</summary>
    public string? Search { get; init; }

    public Guid? VendorId { get; init; }

    public VendorReturnStatus? Status { get; init; }

    public DateOnly? From { get; init; }

    public DateOnly? To { get; init; }

    /// <summary>Whitelisted sort: <c>ReturnDate</c>, <c>Number</c> or <c>CreatedAt</c>, then <c>ASC</c> or <c>DESC</c>.</summary>
    public string? OrderBy { get; init; } = "ReturnDate DESC";

    public int PageIndex { get; init; } = 1;

    public int PageSize { get; init; } = 20;
}

/// <summary>R12: one row of the list.</summary>
public sealed record VendorReturnSummary(
    Guid Id, string Number, DateOnly ReturnDate, Guid VendorId, string VendorCode, string VendorName, decimal GrandTotal, VendorReturnStatus Status, DateTime CreatedAt);

/// <summary>R12: one line as it was kept.</summary>
public sealed record VendorReturnLineResponse(Guid ItemId, string ItemCode, string ItemName, ReturnStockSource StockSource, decimal RateAmount, int Quantity, decimal LineAmount);

/// <summary>R12: one vendor return with its lines and totals.</summary>
public sealed record VendorReturnResponse(
    Guid Id,
    string Number,
    DateOnly ReturnDate,
    string FinancialYear,
    Guid VendorId,
    string VendorCode,
    string VendorName,
    string? Remark,
    decimal Subtotal,
    decimal RoundOff,
    decimal GrandTotal,
    VendorReturnStatus Status,
    string? CancelReason,
    DateOnly? CancelledOn,
    IReadOnlyList<VendorReturnLineResponse> Lines,
    DateTime CreatedAt);
