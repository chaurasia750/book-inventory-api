namespace BookInventory.Domain.Payment;

/// <summary>The message codes of the <c>payment</c> module; their human text is in the module's catalogue.</summary>
public static class PaymentMessages
{
    public const string ReceiptNotFound = "payment.receipt_not_found";
    public const string SchoolNotFound = "payment.school_not_found";
    public const string InvalidRequestId = "payment.invalid_request_id";
    public const string InvalidDate = "payment.invalid_date";
    public const string InvalidModes = "payment.invalid_modes";
    public const string InvalidAmount = "payment.invalid_amount";
    public const string InvalidReference = "payment.invalid_reference";
    public const string InvalidCheque = "payment.invalid_cheque";
    public const string InvalidRemark = "payment.invalid_remark";
    public const string InvalidAllocation = "payment.invalid_allocation";
    public const string InvoiceNotOpen = "payment.invoice_not_open";
    public const string TooManyIds = "payment.too_many_ids";
    public const string ChequeNotFound = "payment.cheque_not_found";
    public const string ChequeResultSet = "payment.cheque_result_set";
    public const string InvalidResultDate = "payment.invalid_result_date";
    public const string InvalidReason = "payment.invalid_reason";
    public const string ReceiptNotRecorded = "payment.receipt_not_recorded";
    public const string NoAdvance = "payment.no_advance";
    public const string ProofInvalid = "payment.proof_invalid";
    public const string ProofNotFound = "payment.proof_not_found";
    public const string ReceiptHasChequeResult = "payment.receipt_has_cheque_result";
}
