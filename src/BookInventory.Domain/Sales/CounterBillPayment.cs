using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.Domain.Core;

namespace BookInventory.Domain.Sales;

/// <summary>Spec sales R45: one Cash or UPI payment of a counter bill; insert-only, never changed.</summary>
public class CounterBillPayment : CompanyOwnedChild
{
    public const int ReferenceMaxLength = 50;

    private CounterBillPayment()
    {
    }

    public Guid CounterBillId { get; private set; }

    public CounterPaymentMode Mode { get; private set; }

    public decimal Amount { get; private set; }

    public string? Reference { get; private set; }

    /// <summary>R45: an amount above zero with at most two decimals and a reference of at most 50 characters.</summary>
    internal static CounterBillPayment Of(Guid billId, CounterPaymentInput input)
    {
        var reference = input.Reference?.Trim();
        if (input.Amount <= 0m || !SaleMoney.HasAtMostTwoDecimals(input.Amount) || input.Amount > SaleMoney.MaxAmount
            || reference?.Length > ReferenceMaxLength || !Enum.IsDefined(input.Mode))
            throw new ValidationFailureException(CounterMessages.InvalidPayment);

        return new CounterBillPayment
        {
            CounterBillId = billId,
            Mode = input.Mode,
            Amount = input.Amount,
            Reference = string.IsNullOrEmpty(reference) ? null : reference
        };
    }
}
