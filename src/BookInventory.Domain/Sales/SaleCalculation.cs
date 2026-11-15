using BookInventory.BuildingBlocks.Domain.Exceptions;

namespace BookInventory.Domain.Sales;

/// <summary>
/// Spec sales R7, D2 (BR-DIS-03 to BR-DIS-05, BR-GST-04, BR-GST-05): the one rule that turns lines, a total discount and a GST
/// mode into amounts, used by the order and the invoice so they can never compute differently. Every step is rounded to 2
/// decimals half away from zero; the paise left by sharing the total discount go to the largest line (the first on a tie); the
/// grand total is rounded to the nearest rupee and the difference is the round-off.
/// </summary>
public static class SaleCalculation
{
    public static SaleAmounts Compute(IReadOnlyList<SaleLineInput> lines, Discount totalDiscount, GstMode gstMode)
    {
        var bases = lines.Select(LineBase).ToList();
        var subtotal = bases.Sum(b => b.LineAmount);
        var discount = totalDiscount.Kind == DiscountKind.Amount && totalDiscount.Value > subtotal
            ? throw new ValidationFailureException(SalesMessages.DiscountTooLarge)
            : totalDiscount.AmountOn(subtotal);
        var shares = Shares(bases.Select(b => b.LineAmount).ToList(), subtotal, discount);

        var amounts = bases.Select((b, i) => WithTax(b, shares[i], lines[i].GstRatePercent, gstMode)).ToList();
        var taxable = amounts.Sum(a => a.Taxable);
        var (cgst, sgst, igst) = (amounts.Sum(a => a.Cgst), amounts.Sum(a => a.Sgst), amounts.Sum(a => a.Igst));
        var before = taxable + cgst + sgst + igst;
        var grand = SaleMoney.RoundRupee(before);
        return new SaleAmounts(amounts, new SaleTotals(subtotal, discount, taxable, cgst, sgst, igst, grand - before, grand));
    }

    private static SaleLineAmounts LineBase(SaleLineInput line)
    {
        var gross = SaleMoney.Round2(line.Rate * line.Quantity);
        var itemDiscount = line.ItemDiscount.AmountOn(gross);
        return new SaleLineAmounts(gross, itemDiscount, gross - itemDiscount, 0m, 0m, 0m, 0m, 0m);
    }

    /// <summary>R7 step 3 (BR-DIS-04): the total discount shared by line amount; the rounding rest goes to the largest line.</summary>
    private static decimal[] Shares(IReadOnlyList<decimal> amounts, decimal subtotal, decimal discount)
    {
        var shares = new decimal[amounts.Count];
        if (subtotal == 0m || discount == 0m)
            return shares;

        for (var i = 0; i < amounts.Count; i++)
            shares[i] = SaleMoney.Round2(discount * amounts[i] / subtotal);
        var largest = amounts.Select((a, i) => (a, i)).OrderByDescending(x => x.a).ThenBy(x => x.i).First().i;
        shares[largest] += discount - shares.Sum();
        return shares;
    }

    private static SaleLineAmounts WithTax(SaleLineAmounts line, decimal share, decimal ratePercent, GstMode mode)
    {
        var taxable = line.LineAmount - share;
        var half = mode == GstMode.SameState ? SaleMoney.Round2(taxable * ratePercent / 2m / 100m) : 0m;
        var igst = mode == GstMode.OtherState ? SaleMoney.Round2(taxable * ratePercent / 100m) : 0m;
        return line with { TotalDiscountShare = share, Taxable = taxable, Cgst = half, Sgst = half, Igst = igst };
    }
}
