using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.Domain.Core;
using ChequeState = BookInventory.Domain.Payment.ChequeStatus;

namespace BookInventory.Domain.Payment;

/// <summary>
/// Spec payment R5: one mode of a <see cref="Receipt"/>: its amount and its reference (UPI transaction id, UTR, or the cheque's
/// number, date and bank). A company-owned child (platform P18), changed only by its receipt. A cheque starts <c>PENDING</c>.
/// </summary>
public class ReceiptMode : CompanyOwnedChild
{
    public const int ReferenceMinLength = 4;
    public const int ReferenceMaxLength = 50;
    public const int ChequeNumberMinLength = 4;
    public const int ChequeNumberMaxLength = 20;
    public const int BankNameMaxLength = 100;

    private ReceiptMode()
    {
    }

    public Guid ReceiptId { get; private set; }

    public ModeType ModeType { get; private set; }

    public decimal Amount { get; private set; }

    public string? Reference { get; private set; }

    public string? ChequeNumber { get; private set; }

    public DateOnly? ChequeDate { get; private set; }

    public string? BankName { get; private set; }

    public ChequeStatus? ChequeStatus { get; private set; }

    public DateOnly? ResultDate { get; private set; }

    public string? BounceReason { get; private set; }

    internal static ReceiptMode Of(Guid receiptId, ReceiptModeInput input)
    {
        var mode = new ReceiptMode { ReceiptId = receiptId, ModeType = input.Type, Amount = PaymentMoney.Require(input.Amount) };
        switch (input.Type)
        {
            case ModeType.Upi or ModeType.BankTransfer:
                mode.Reference = Text(input.Reference, ReferenceMinLength, ReferenceMaxLength, PaymentMessages.InvalidReference);
                break;
            case ModeType.Cheque:
                mode.ChequeNumber = Text(input.ChequeNumber, ChequeNumberMinLength, ChequeNumberMaxLength, PaymentMessages.InvalidCheque);
                mode.ChequeDate = input.ChequeDate ?? throw new ValidationFailureException(PaymentMessages.InvalidCheque);
                mode.BankName = Text(input.BankName, 1, BankNameMaxLength, PaymentMessages.InvalidCheque);
                mode.ChequeStatus = ChequeState.Pending;
                break;
        }

        return mode;
    }

    private static string Text(string? value, int min, int max, string code)
    {
        var trimmed = value?.Trim() ?? string.Empty;
        return trimmed.Length >= min && trimmed.Length <= max ? trimmed : throw new ValidationFailureException(code);
    }
}
