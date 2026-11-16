namespace BookInventory.Domain.Sales;

/// <summary>Spec sales R53-R56, R59: everything a credit note is made from: its number and date, the school, the invoice (optional) and the lines.</summary>
public sealed record CreditNoteInput
{
    public required string Number { get; init; }

    public required DateOnly NoteDate { get; init; }

    public required DateOnly Today { get; init; }

    public required SaleSchool School { get; init; }

    public string? SchoolGstin { get; init; }

    public required CompanyInvoiceSnapshot CompanySnapshot { get; init; }

    public required string Session { get; init; }

    public ReturnedInvoice? Invoice { get; init; }

    public required bool IsGstApplied { get; init; }

    public required GstMode GstMode { get; init; }

    public required CreditNoteReason Reason { get; init; }

    public string? Remark { get; init; }

    public required IReadOnlyList<CreditNoteLineInput> Lines { get; init; }
}
