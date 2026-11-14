using BookInventory.BuildingBlocks.Domain.Exceptions;

namespace BookInventory.Domain.Price;

/// <summary>R11: a change is a percentage of the current value or an amount in rupees.</summary>
public enum PriceChangeKind
{
    Percent,

    Amount
}

/// <summary>R11: the change raises or lowers the value.</summary>
public enum PriceChangeDirection
{
    Increase,

    Decrease
}

/// <summary>Why a change was not applied to a value (R11).</summary>
public enum PriceChangeSkip
{
    BelowZero,

    Overflow
}

/// <summary>The result of applying a <see cref="PriceChange"/> to one value: the new value, or why it was skipped.</summary>
public readonly record struct PriceChangeOutcome(decimal? NewValue, PriceChangeSkip? Skipped);

/// <summary>
/// R11, R17: one increase or decrease of one price field. A percentage is of the current value of that same field; the result is
/// computed in exact decimal arithmetic and rounded once, at the end, to two decimals with halves away from zero (the owner's
/// answer to Q1: <c>2.525</c> becomes <c>2.53</c>). A result below zero, or above what <c>numeric(18,2)</c> holds, is not applied.
/// The value itself is zero or more with at most two decimals.
/// </summary>
public sealed record PriceChange
{
    public PriceChange(PriceChangeKind kind, PriceChangeDirection direction, decimal value)
    {
        if (value < 0m || decimal.Round(value, 2) != value)
            throw new ValidationFailureException(PriceMessages.InvalidChange);

        Kind = kind;
        Direction = direction;
        Value = value;
    }

    public PriceChangeKind Kind { get; }

    public PriceChangeDirection Direction { get; }

    public decimal Value { get; }

    public PriceChangeOutcome Apply(decimal current)
    {
        var delta = Kind == PriceChangeKind.Percent ? current * Value / 100m : Value;
        var exact = Direction == PriceChangeDirection.Increase ? current + delta : current - delta;
        var rounded = Math.Round(exact, 2, MidpointRounding.AwayFromZero);

        if (rounded < 0m)
            return new PriceChangeOutcome(null, PriceChangeSkip.BelowZero);

        return rounded > PriceRules.MaxAmount
            ? new PriceChangeOutcome(null, PriceChangeSkip.Overflow)
            : new PriceChangeOutcome(rounded, null);
    }
}
