namespace BookInventory.Application.Finance;

/// <summary>
/// Spec finance R18: the ledger statement's HTML + CSS templates live in this module as embedded resources (Scriban text), never
/// in the shared library. They are read once and kept.
/// </summary>
internal static class LedgerPdfTemplates
{
    public static string Body { get; } = Read("ledger-body.sbn");

    public static string Header { get; } = Read("ledger-header.sbn");

    public static string Footer { get; } = Read("ledger-footer.sbn");

    private static string Read(string name)
    {
        var assembly = typeof(LedgerPdfTemplates).Assembly;
        var resource = assembly.GetManifestResourceNames().Single(n => n.EndsWith("." + name, StringComparison.Ordinal));
        using var stream = assembly.GetManifestResourceStream(resource)!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
