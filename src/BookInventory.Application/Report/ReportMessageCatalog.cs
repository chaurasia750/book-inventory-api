using BookInventory.Domain.Report;

namespace BookInventory.Application.Report;

/// <summary>The human text of every <see cref="ReportMessages"/> code; the code travels, the text is looked up (R55 of iam).</summary>
public static class ReportMessageCatalog
{
    public static IReadOnlyDictionary<string, string> Messages { get; } =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [ReportMessages.InvalidSession] = "The session must look like 2026-27.",
            [ReportMessages.PartFailed] = "This part of the dashboard could not be read. Try again."
        };
}
