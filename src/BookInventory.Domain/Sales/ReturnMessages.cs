namespace BookInventory.Domain.Sales;

/// <summary>
/// Message codes of the school return: the credit note and the refund (spec sales R53-R69). The human text lives in
/// <c>BookInventory.Application.Sales.ReturnMessageCatalog</c>; the codes are stable API surface.
/// </summary>
public static class ReturnMessages
{
    public const string CreditNoteNotFound = "sales.credit_note.not_found";
    public const string InvalidDate = "sales.credit_note.invalid_date";
    public const string NoLines = "sales.credit_note.no_lines";
    public const string TooManyLines = "sales.credit_note.too_many_lines";
    public const string InvalidReason = "sales.credit_note.invalid_reason";
    public const string RemarkRequired = "sales.credit_note.remark_required";
    public const string InvoiceCancelled = "sales.credit_note.invoice_cancelled";
    public const string InvoiceOfOtherSchool = "sales.credit_note.invoice_other_school";
    public const string InvoiceLineUnknown = "sales.credit_note.invoice_line_unknown";
    public const string AlreadyCancelled = "sales.credit_note.already_cancelled";
    public const string InvoiceHasCreditNotes = "sales.invoice.has_credit_notes";
    public const string SchoolNotFound = "sales.credit_note.school_not_found";
    public const string RefundNotFound = "sales.refund.not_found";
    public const string RefundInvalidDate = "sales.refund.invalid_date";
    public const string RefundInvalidAmount = "sales.refund.invalid_amount";
    public const string RefundInvalidMode = "sales.refund.invalid_mode";
    public const string RefundInvalidReference = "sales.refund.invalid_reference";
    public const string RefundExceedsCredit = "sales.refund.exceeds_credit";
    public const string RefundCreditNoteOfOtherSchool = "sales.refund.credit_note_other_school";
    public const string RefundAlreadyCancelled = "sales.refund.already_cancelled";
}
