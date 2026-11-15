namespace BookInventory.Domain.Payment;

/// <summary>Spec payment R4-R9: everything a receipt is recorded from: the numbers, the school, the dates, the modes and the open invoices it may be applied to.</summary>
public sealed record ReceiptDraft
{
    public required Guid ClientRequestId { get; init; }
    public required string Number { get; init; }
    public required string FinancialYear { get; init; }
    public required Guid SchoolId { get; init; }
    public required DateOnly ReceiptDate { get; init; }
    public required DateOnly Today { get; init; }
    public required IReadOnlyList<ReceiptModeInput> Modes { get; init; }
    public required IReadOnlyList<OpenInvoice> OpenInvoices { get; init; }
    public IReadOnlyList<AllocationInput>? Allocations { get; init; }
    public string? Remark { get; init; }
}
