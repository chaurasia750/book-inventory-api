using BookInventory.BuildingBlocks.Domain.Exceptions;

namespace BookInventory.Domain.Payment;

/// <summary>Spec payment R4, R5: an amount of a payment: above zero, at most two decimals, at most 9,999,999,999.99; checked in one place.</summary>
public static class PaymentMoney
{
    public const decimal MaxAmount = 9_999_999_999.99m;

    public static decimal Require(decimal amount, string code = PaymentMessages.InvalidAmount) =>
        amount > 0m && amount <= MaxAmount && decimal.Round(amount, 2) == amount ? amount : throw new ValidationFailureException(code);
}
