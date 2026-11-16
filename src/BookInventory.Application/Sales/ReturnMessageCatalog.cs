using BookInventory.Domain.Sales;

namespace BookInventory.Application.Sales;

/// <summary>The human text of every <see cref="ReturnMessages"/> code; the code travels, the text is looked up (R55 of iam).</summary>
public static class ReturnMessageCatalog
{
    public static IReadOnlyDictionary<string, string> Messages { get; } =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [ReturnMessages.CreditNoteNotFound] = "The credit note could not be found.",
            [ReturnMessages.InvalidDate] = "The date cannot be in the future or before the invoice.",
            [ReturnMessages.NoLines] = "Add at least one item to the return.",
            [ReturnMessages.TooManyLines] = "A credit note can have at most 500 lines.",
            [ReturnMessages.InvalidReason] = "Choose a valid reason and a Good or Damaged condition for each line.",
            [ReturnMessages.RemarkRequired] = "Write a remark when the reason is Other.",
            [ReturnMessages.InvoiceCancelled] = "This invoice is cancelled, nothing can be returned against it.",
            [ReturnMessages.InvoiceOfOtherSchool] = "The invoice belongs to another school.",
            [ReturnMessages.InvoiceLineUnknown] = "Each line must name a line of the chosen invoice, or an item when no invoice is chosen.",
            [ReturnMessages.AlreadyCancelled] = "The credit note is already cancelled.",
            [ReturnMessages.InvoiceHasCreditNotes] = "This invoice has a credit note. Cancel the credit note first.",
            [ReturnMessages.SchoolNotFound] = "The school could not be found.",
            [ReturnMessages.RefundNotFound] = "The refund could not be found.",
            [ReturnMessages.RefundInvalidDate] = "The refund date cannot be in the future.",
            [ReturnMessages.RefundInvalidAmount] = "The amount must be above zero with at most two decimals.",
            [ReturnMessages.RefundInvalidMode] = "Choose Cash, UPI or Bank transfer.",
            [ReturnMessages.RefundInvalidReference] = "A cash refund has no reference; a UPI or bank reference is 4 to 50 characters.",
            [ReturnMessages.RefundExceedsCredit] = "The refund is more than what the school is owed.",
            [ReturnMessages.RefundCreditNoteOfOtherSchool] = "Choose a live credit note of the same school.",
            [ReturnMessages.RefundAlreadyCancelled] = "The refund is already cancelled."
        };
}
