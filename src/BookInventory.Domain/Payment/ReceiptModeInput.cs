namespace BookInventory.Domain.Payment;

/// <summary>Spec payment R5: one mode of a receipt as it arrives; <see cref="ReceiptMode"/> validates it.</summary>
public sealed record ReceiptModeInput(
    ModeType Type,
    decimal Amount,
    string? Reference = null,
    string? ChequeNumber = null,
    DateOnly? ChequeDate = null,
    string? BankName = null);
