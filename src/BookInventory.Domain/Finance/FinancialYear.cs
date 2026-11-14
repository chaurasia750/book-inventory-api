namespace BookInventory.Domain.Finance;

/// <summary>Spec finance R4 (BR-SES-01, BR-SES-02): the financial year is April to March of a date, written <c>2026-27</c>.</summary>
public static class FinancialYear
{
    public static string Of(DateOnly date)
    {
        var start = date.Month >= 4 ? date.Year : date.Year - 1;
        return $"{start}-{(start + 1) % 100:D2}";
    }

    /// <summary>The first day of the financial year a label names, or null for a label that is not <c>yyyy-yy</c> with consecutive years.</summary>
    public static DateOnly? StartOf(string? label)
    {
        if (label is not { Length: 7 } || label[4] != '-'
            || !int.TryParse(label[..4], out var start) || !int.TryParse(label[5..], out var end)
            || (start + 1) % 100 != end || start < 2000)
            return null;

        return new DateOnly(start, 4, 1);
    }
}
