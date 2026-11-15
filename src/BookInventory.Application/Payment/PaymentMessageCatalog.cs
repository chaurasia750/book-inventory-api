using BookInventory.Domain.Payment;

namespace BookInventory.Application.Payment;

/// <summary>The human text of every <see cref="PaymentMessages"/> code; the code travels, the text is looked up.</summary>
public static class PaymentMessageCatalog
{
    public static IReadOnlyDictionary<string, string> Messages { get; } =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [PaymentMessages.ReceiptNotFound] = "The receipt could not be found.",
            [PaymentMessages.SchoolNotFound] = "The school could not be found.",
            [PaymentMessages.InvalidRequestId] = "A payment needs a request id so that a retry cannot record it twice.",
            [PaymentMessages.InvalidDate] = "The receipt date cannot be in the future.",
            [PaymentMessages.InvalidModes] = "A receipt needs one to ten payment modes.",
            [PaymentMessages.InvalidAmount] = "An amount must be above zero, with at most two decimals.",
            [PaymentMessages.InvalidReference] = "A UPI transaction id or a bank UTR number must have 4 to 50 characters.",
            [PaymentMessages.InvalidCheque] = "A cheque needs its number (4 to 20 characters), its date and the school's bank.",
            [PaymentMessages.InvalidRemark] = "The remark can have at most 500 characters.",
            [PaymentMessages.InvalidAllocation] = "The amount applied to an invoice must be above zero, at most what is due on it, and all together at most the payment.",
            [PaymentMessages.InvoiceNotOpen] = "This invoice is not an open invoice of the school.",
            [PaymentMessages.ChequeNotFound] = "The cheque could not be found in this receipt.",
            [PaymentMessages.ChequeResultSet] = "This cheque already has a result (cleared or bounced).",
            [PaymentMessages.InvalidResultDate] = "The date cannot be in the future, and a cheque cannot clear before its cheque date.",
            [PaymentMessages.InvalidReason] = "Give the reason, 5 to 500 characters.",
            [PaymentMessages.ReceiptNotRecorded] = "Only a recorded receipt can be changed; this one was cancelled.",
            [PaymentMessages.NoAdvance] = "This receipt has no advance to apply.",
            [PaymentMessages.ReceiptHasChequeResult] = "A receipt with a cleared or bounced cheque cannot be cancelled.",
            [PaymentMessages.TooManyIds] = "At most 100 invoices can be asked at once."
        };
}
