namespace BookInventory.Domain.Sales;

/// <summary>Spec sales R64: everything a refund is made from: its number, date, school, amount, mode, reference and the credit note it settles.</summary>
public sealed record RefundInput
{
    public required string Number { get; init; }

    public required DateOnly RefundDate { get; init; }

    public required DateOnly Today { get; init; }

    public required SaleSchool School { get; init; }

    public required decimal Amount { get; init; }

    public required RefundMode Mode { get; init; }

    public string? Reference { get; init; }

    public Guid? CreditNoteId { get; init; }

    public string? CreditNoteNumber { get; init; }

    public string? Remark { get; init; }
}
