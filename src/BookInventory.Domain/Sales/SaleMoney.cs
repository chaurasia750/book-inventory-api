namespace BookInventory.Domain.Sales;

/// <summary>
/// Spec sales R7, R17 (owner's answer Q5, Q7): every money step is rounded to 2 decimals half away from zero; the grand total is
/// rounded to the nearest rupee the same way. Written once for the order and the invoice.
/// </summary>
public static class SaleMoney
{
    /// <summary>The largest amount <c>numeric(18,2)</c> holds.</summary>
    public const decimal MaxAmount = 9_999_999_999_999_999.99m;

    public static decimal Round2(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);

    public static decimal RoundRupee(decimal value) => Math.Round(value, 0, MidpointRounding.AwayFromZero);

    public static bool HasAtMostTwoDecimals(decimal value) => decimal.Round(value, 2) == value;

    /// <summary>R7 (BR-GST-05, owner's answer Q6): same state when both states are equal ignoring case, or when either is missing.</summary>
    public static GstMode GstModeOf(bool isGstApplied, string? companyState, string? schoolState)
    {
        if (!isGstApplied)
            return GstMode.NotApplied;
        if (string.IsNullOrWhiteSpace(companyState) || string.IsNullOrWhiteSpace(schoolState))
            return GstMode.SameState;

        return string.Equals(companyState.Trim(), schoolState.Trim(), StringComparison.OrdinalIgnoreCase) ? GstMode.SameState : GstMode.OtherState;
    }
}
