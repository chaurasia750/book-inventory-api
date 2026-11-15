using System.Reflection;

namespace BookInventory.Application.Sales;

/// <summary>
/// Spec sales R28: the invoice's HTML + CSS templates live in this module as embedded resources (Scriban text), never in the
/// shared library. They are read once and kept.
/// </summary>
internal static class InvoicePdfTemplates
{
    public static string Body { get; } = Read("invoice-body.sbn");

    public static string Header { get; } = Read("invoice-header.sbn");

    public static string Footer { get; } = Read("invoice-footer.sbn");

    private static string Read(string name)
    {
        var assembly = typeof(InvoicePdfTemplates).Assembly;
        var resource = assembly.GetManifestResourceNames().Single(n => n.EndsWith("." + name, StringComparison.Ordinal));
        using var stream = assembly.GetManifestResourceStream(resource)!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
