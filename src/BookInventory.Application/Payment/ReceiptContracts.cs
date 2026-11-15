using System.ComponentModel.DataAnnotations;
using BookInventory.Domain.Payment;

namespace BookInventory.Application.Payment;

/// <summary>R5: one mode of a receipt as the caller sends it; the aggregate checks the values.</summary>
public sealed record ReceiptModeRequest(
    [param: Required] ModeType? Type,
    [param: Required] decimal? Amount,
    string? Reference = null,
    string? ChequeNumber = null,
    DateOnly? ChequeDate = null,
    string? BankName = null)
{
    public ReceiptModeInput ToInput() => new(Type!.Value, Amount!.Value, Reference, ChequeNumber, ChequeDate, BankName);
}

/// <summary>R6: an amount the caller applies to one invoice by hand.</summary>
public sealed record AllocationRequest([param: Required] Guid? InvoiceId, [param: Required] decimal? Amount)
{
    public AllocationInput ToInput() => new(InvoiceId!.Value, Amount!.Value);
}

/// <summary>R4-R9: a school payment. <c>ClientRequestId</c> makes a retry harmless (R9); no allocations means oldest invoice first (R6).</summary>
public sealed record RecordReceiptRequest(
    [param: Required] Guid? ClientRequestId,
    [param: Required] Guid? SchoolId,
    DateOnly? ReceiptDate,
    [param: Required] IReadOnlyList<ReceiptModeRequest>? Modes,
    IReadOnlyList<AllocationRequest>? Allocations = null,
    string? Remark = null);

/// <summary>R16: the filters of the receipt list.</summary>
public sealed record ReceiptQuery
{
    /// <summary>Free text over the receipt number.</summary>
    public string? Search { get; init; }

    public Guid? SchoolId { get; init; }

    public ReceiptStatus? Status { get; init; }

    public DateOnly? From { get; init; }

    public DateOnly? To { get; init; }

    /// <summary>Only receipts that have a mode of this type.</summary>
    public ModeType? Mode { get; init; }

    /// <summary>Only receipts that have a cheque in this status (the pending cheques are <c>PENDING</c>).</summary>
    public ChequeStatus? ChequeStatus { get; init; }

    public int PageIndex { get; init; } = 1;

    public int PageSize { get; init; } = 20;
}

/// <summary>R16: one list row as the store reads it; the school's code and name are added from the school directory.</summary>
public sealed record ReceiptRow(Guid Id, string Number, DateOnly ReceiptDate, Guid SchoolId, decimal TotalAmount, ReceiptStatus Status);

/// <summary>R16: one list row of the receipts.</summary>
public sealed record ReceiptSummary(Guid Id, string Number, DateOnly ReceiptDate, Guid SchoolId, string? SchoolCode, string? SchoolName, decimal TotalAmount, ReceiptStatus Status);

/// <summary>R5, R16: one mode of a receipt as read.</summary>
public sealed record ReceiptModeDto(
    Guid Id, ModeType Type, decimal Amount, string? Reference, string? ChequeNumber, DateOnly? ChequeDate, string? BankName, ChequeStatus? ChequeStatus);

/// <summary>R6, R16: the net amount applied to one invoice (the sum of its allocation rows of this receipt); an invoice taken back to zero is not listed.</summary>
public sealed record ReceiptAllocationDto(Guid InvoiceId, string? InvoiceNumber, decimal Amount);

/// <summary>R16: a receipt with its modes, allocations and advance.</summary>
public sealed record ReceiptDetail(
    ReceiptSummary Summary,
    string? Remark,
    decimal AllocatedAmount,
    decimal AdvanceAmount,
    IReadOnlyList<ReceiptModeDto> Modes,
    IReadOnlyList<ReceiptAllocationDto> Allocations);

/// <summary>R9: the answer of recording a receipt: the new receipt, or only the id of the one already recorded for the same client request.</summary>
public sealed record RecordedReceipt(Guid Id, ReceiptDetail? Detail);

/// <summary>R3: an open invoice of a school with what was applied to it and what is still due.</summary>
public sealed record OpenInvoiceDto(Guid InvoiceId, string Number, DateOnly InvoiceDate, decimal GrandTotal, decimal Allocated, decimal Due);

/// <summary>R3: a school's balance (finance R7; negative when it has paid in advance) and its open invoices, oldest first.</summary>
public sealed record DuesResponse(Guid SchoolId, decimal Balance, IReadOnlyList<OpenInvoiceDto> OpenInvoices);

/// <summary>R16: what an invoice has been applied and what is still due on it (0 for a cancelled invoice).</summary>
public sealed record InvoiceDueDto(Guid InvoiceId, decimal GrandTotal, decimal Allocated, decimal Due);
