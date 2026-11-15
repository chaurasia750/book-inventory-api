using System.Text.RegularExpressions;

namespace BookInventory.Application.Sales;

/// <summary>
/// Spec sales R35: the terms and conditions of a company are one fixed text that may hold placeholders written <c>{{name}}</c>
/// (spaces inside the braces are allowed). When the invoice is printed each placeholder is replaced by the invoice's own value; a
/// placeholder that is not in the list is left as typed, so a typing mistake stays visible.
/// </summary>
public static partial class InvoiceTerms
{
    public static string? Resolve(string? text, IReadOnlyDictionary<string, string> values) =>
        string.IsNullOrEmpty(text) ? text : Placeholder().Replace(text, match => values.TryGetValue(match.Groups[1].Value, out var value) ? value : match.Value);

    [GeneratedRegex(@"\{\{\s*([a-z_]+)\s*\}\}")]
    private static partial Regex Placeholder();
}
