namespace BookInventory.Domain.Sales;

/// <summary>Spec sales R42-R46: everything a counter bill is made from: its number and date, the counter, the snapshots, the lines and the payments.</summary>
public sealed record CounterBillInput
{
    public required string Number { get; init; }
    public required DateOnly BillDate { get; init; }
    public required DateOnly Today { get; init; }
    public required Guid CounterId { get; init; }
    public required string Session { get; init; }
    public required SaleSchool School { get; init; }
    public string? SchoolGstin { get; init; }
    public required StudentSnapshot Student { get; init; }
    public required CompanyInvoiceSnapshot CompanySnapshot { get; init; }
    public required bool IsGstApplied { get; init; }
    public required GstMode GstMode { get; init; }
    public Discount TotalDiscount { get; init; } = Discount.None;
    public required IReadOnlyList<CounterLineInput> Lines { get; init; }
    public required IReadOnlyList<CounterPaymentInput> Payments { get; init; }
    public string? Remark { get; init; }
}
