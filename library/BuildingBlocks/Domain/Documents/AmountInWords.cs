using System.Text;

namespace BookInventory.BuildingBlocks.Domain.Documents;

/// <summary>
/// An amount written in words in the Indian numbering system (thousand, lakh, crore) for the printed
/// invoice. Rupees are always written; paise are added only when the amount has them (the grand total never does, but the
/// same helper serves any money value). Written once, used by the PDF and, later, the invoice screen.
/// </summary>
public static class AmountInWords
{
    private static readonly string[] Ones =
    [
        "Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten",
        "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen"
    ];

    private static readonly string[] Tens =
        ["", "", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety"];

    /// <summary>The Indian scale names above thousand, two digits each; the index is the group's place.</summary>
    private static readonly string[] Scales =
        ["Thousand", "Lakh", "Crore", "Arab", "Kharab", "Neel", "Padma", "Shankh"];

    /// <summary>R32: e.g. <c>Rupees Twenty-Two Thousand Seven Hundred Twenty-Five Only</c>.</summary>
    public static string Of(decimal amount)
    {
        var rounded = Math.Round(amount, 2, MidpointRounding.AwayFromZero);
        var sign = rounded < 0m ? "Minus " : string.Empty;
        var value = Math.Abs(rounded);
        var rupees = decimal.Truncate(value);
        var paise = (int)Math.Round((value - rupees) * 100m, MidpointRounding.AwayFromZero);

        var text = new StringBuilder(sign).Append("Rupees ").Append(Indian(rupees));
        if (paise > 0)
            text.Append(" and Paise ").Append(Indian(paise));
        return text.Append(" Only").ToString();
    }

    /// <summary>The whole part in Indian grouping: the last three digits, then two digits per scale name.</summary>
    private static string Indian(decimal number)
    {
        var whole = (long)number;
        if (whole == 0)
            return Ones[0];

        var parts = new List<string>();
        if (whole % 1000 is var last and > 0)
            parts.Add(BelowThousand((int)last));
        whole /= 1000;

        for (var scale = 0; whole > 0 && scale < Scales.Length; scale++)
        {
            if (whole % 100 is var group and > 0)
                parts.Add($"{BelowHundred((int)group)} {Scales[scale]}");
            whole /= 100;
        }

        // Beyond the named scales (never for a money column of numeric(18,2)); kept so the words are still complete.
        if (whole > 0)
            parts.Add(whole.ToString(System.Globalization.CultureInfo.InvariantCulture));

        parts.Reverse();
        return string.Join(" ", parts);
    }

    private static string BelowThousand(int number)
    {
        var parts = new List<string>();
        if (number >= 100)
        {
            parts.Add($"{Ones[number / 100]} Hundred");
            number %= 100;
        }

        if (number > 0)
            parts.Add(BelowHundred(number));
        return string.Join(" ", parts);
    }

    private static string BelowHundred(int number) =>
        number < 20
            ? Ones[number]
            : number % 10 == 0
                ? Tens[number / 10]
                : $"{Tens[number / 10]}-{Ones[number % 10]}";
}
