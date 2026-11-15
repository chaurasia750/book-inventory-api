namespace BookInventory.Domain.Sales;

/// <summary>Spec sales R2: the order's header values as the use case resolved them (school, date, session, GST).</summary>
public sealed record OrderHeader
{
    public required SaleSchool School { get; init; }

    public required DateOnly OrderDate { get; init; }

    public required string Session { get; init; }

    public required bool IsGstApplied { get; init; }

    public required GstMode GstMode { get; init; }

    public Discount TotalDiscount { get; init; } = Discount.None;

    public string? Remark { get; init; }
}
