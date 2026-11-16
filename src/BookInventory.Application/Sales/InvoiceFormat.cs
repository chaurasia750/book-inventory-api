using System.Globalization;

namespace BookInventory.Application.Sales;

/// <summary>Spec sales R30-R33: how the printed invoice writes money and dates (Indian digit grouping, <c>1,34,537.00</c>), written once.</summary>
public static class InvoiceFormat
{
    /// <summary>Two decimals and the Indian grouping: the last three digits, then pairs.</summary>
    public static string Money(decimal amount)
    {
        var text = Math.Abs(amount).ToString("0.00", CultureInfo.InvariantCulture);
        var dot = text.IndexOf('.');
        var whole = text[..dot];
        if (whole.Length > 3)
        {
            var groups = new List<string>();
            var rest = whole[..^3];
            while (rest.Length > 2)
            {
                groups.Insert(0, rest[^2..]);
                rest = rest[..^2];
            }

            if (rest.Length > 0)
                groups.Insert(0, rest);
            whole = string.Join(',', groups) + "," + whole[^3..];
        }

        return (amount < 0m ? "-" : string.Empty) + whole + text[dot..];
    }

    /// <summary>Platform P26: an amount standing on its own is written with the rupee symbol, <c>₹ 1,200.00</c>; a negative one is <c>-₹ 1,200.00</c>. Table cells use <see cref="Money"/> and carry the symbol in the column heading.</summary>
    public static string Amount(decimal amount) => (amount < 0m ? "-" : string.Empty) + "₹ " + Money(Math.Abs(amount));

    public static string Date(DateOnly date) => date.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);
}
