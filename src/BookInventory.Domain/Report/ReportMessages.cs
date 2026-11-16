namespace BookInventory.Domain.Report;

/// <summary>
/// Message codes of the report module (spec report R1-R13). The human text lives in <c>BookInventory.Application.Report.ReportMessageCatalog</c>;
/// the codes are stable API surface.
/// </summary>
public static class ReportMessages
{
    public const string InvalidSession = "report.invalid_session";

    /// <summary>R13: a part of the dashboard that could not be read; the caller sees this code in that part only.</summary>
    public const string PartFailed = "report.part_failed";
}
