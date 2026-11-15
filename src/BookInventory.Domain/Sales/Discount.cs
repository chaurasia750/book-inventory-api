using BookInventory.BuildingBlocks.Domain.Exceptions;

namespace BookInventory.Domain.Sales;

/// <summary>
/// Spec sales R3, R7 (BR-DIS-01, BR-DIS-02): an item or total discount: none, a percentage from 0 to 100 or a rupee amount of at
/// least zero, each with at most two decimals. A rupee amount may never exceed what it is taken from.
/// </summary>
public sealed record Discount(DiscountKind Kind, decimal Value)
{
    public static readonly Discount None = new(DiscountKind.None, 0m);

    /// <summary>A checked discount, or <c>400 sales.invalid_discount</c>.</summary>
    public static Discount Of(DiscountKind kind, decimal value)
    {
        var valid = kind switch
        {
            DiscountKind.None => value == 0m,
            DiscountKind.Percent => value is >= 0m and <= 100m && SaleMoney.HasAtMostTwoDecimals(value),
            DiscountKind.Amount => value is >= 0m and <= SaleMoney.MaxAmount && SaleMoney.HasAtMostTwoDecimals(value),
            _ => false
        };
        return valid ? new Discount(kind, value) : throw new ValidationFailureException(SalesMessages.InvalidDiscount);
    }

    /// <summary>R7: the rupee amount taken from <paramref name="baseAmount"/>; a rupee discount above it is <c>400</c>.</summary>
    public decimal AmountOn(decimal baseAmount) =>
        Kind switch
        {
            DiscountKind.Percent => SaleMoney.Round2(baseAmount * Value / 100m),
            DiscountKind.Amount when Value <= baseAmount => Value,
            DiscountKind.Amount => throw new ValidationFailureException(SalesMessages.DiscountTooLarge),
            _ => 0m
        };
}
