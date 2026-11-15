using BookInventory.BuildingBlocks.Domain.Exceptions;

namespace BookInventory.Domain.Sales;

/// <summary>
/// Spec sales R2, R3, R6, R20, R21: the value rules of an order that need none of its state: the header check, merging lines of the
/// same item, price version and class, editing the lines of a confirmed order, deleting them and refusing repeats. Every refusal
/// names the line (R25).
/// </summary>
internal static class OrderRules
{
    /// <summary>R20: applies the edits to the live lines; answers the ids of the edited lines.</summary>
    internal static HashSet<Guid> Edit(IReadOnlyList<SchoolOrderLine> live, IReadOnlyList<OrderLineInput> edits)
    {
        var kept = new HashSet<Guid>();
        foreach (var edit in edits)
        {
            var line = live.FirstOrDefault(l => l.Id == edit.LineId) ?? throw new ValidationFailureException(SalesMessages.LineNotFound) { Subject = edit.Item.Code };
            if (!kept.Add(line.Id))
                throw new ValidationFailureException(SalesMessages.DuplicateLine) { Subject = line.Label };

            line.Edit(edit);
        }

        return kept;
    }

    /// <summary>R20: a line is deleted only when nothing is invoiced on it.</summary>
    internal static void Withdraw(SchoolOrderLine line)
    {
        if (line.InvoicedQuantity > 0)
            throw new ConflictException(SalesMessages.LineInvoiced) { Subject = line.Label };

        line.MarkDeleted();
    }

    /// <summary>R21: one line per item, price version and class.</summary>
    internal static void RequireNoRepeats(IReadOnlyList<SchoolOrderLine> lines)
    {
        var repeated = lines.GroupBy(l => (l.ItemId, l.PriceVersionId, l.ClassName)).FirstOrDefault(g => g.Count() > 1);
        if (repeated is not null)
            throw new ConflictException(SalesMessages.DuplicateLine) { Subject = repeated.First().Label };
    }

    /// <summary>R3, R21: one line per item, price version and class (the same again adds its quantity); whole quantities; at most 500 lines.</summary>
    internal static List<OrderLineInput> Merge(IReadOnlyList<OrderLineInput> lines)
    {
        var bad = lines.FirstOrDefault(l => l.Quantity is < 1 or > SchoolOrder.MaxQuantity);
        if (bad is not null)
            throw new ValidationFailureException(SalesMessages.InvalidQuantity) { Subject = LabelOf(bad) };

        var merged = lines
            .GroupBy(l => (l.Item.Id, l.Price.VersionId, l.ClassName))
            .Select(g => g.First() with { Quantity = g.Sum(l => l.Quantity) })
            .ToList();
        if (merged.Count > SchoolOrder.MaxLines)
            throw new ValidationFailureException(SalesMessages.TooManyLines);

        var over = merged.FirstOrDefault(l => l.Quantity > SchoolOrder.MaxQuantity);
        if (over is not null)
            throw new ValidationFailureException(SalesMessages.InvalidQuantity) { Subject = LabelOf(over) };

        return merged;
    }

    private static string LabelOf(OrderLineInput line) => line.ClassName is null ? line.Item.Code : $"{line.Item.Code} / {line.ClassName}";

    /// <summary>R2, R6: the date is not in the future, the session is written, the remark is short.</summary>
    internal static void RequireHeader(OrderHeader header, DateOnly today)
    {
        if (header.OrderDate == default || header.OrderDate > today)
            throw new ValidationFailureException(SalesMessages.InvalidDate);
        if (string.IsNullOrWhiteSpace(header.Session) || header.Session.Trim().Length > SchoolOrder.SessionMaxLength)
            throw new ValidationFailureException(SalesMessages.InvalidSession);
        if (header.Remark?.Trim().Length > SchoolOrder.RemarkMaxLength)
            throw new ValidationFailureException(SalesMessages.InvalidText);
    }
}
