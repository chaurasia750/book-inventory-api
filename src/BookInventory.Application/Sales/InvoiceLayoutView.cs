using BookInventory.Domain.Sales;

namespace BookInventory.Application.Sales;

/// <summary>
/// R26: shows the lines of one invoice in a layout without changing the invoice. By class: a group per class with its subtotal,
/// lines without a class last. Book list: one group; lines of the same item, price version, rate and item discount become one line
/// (quantities and amounts added); a line that differs in any of these stays separate.
/// </summary>
public static class InvoiceLayoutView
{
    public static IReadOnlyList<InvoiceGroupView> Of(InvoiceLayout layout, IReadOnlyList<InvoiceLineView> lines) =>
        layout == InvoiceLayout.ByClass ? ByClass(lines) : [Group(null, Merged(lines))];

    private static List<InvoiceGroupView> ByClass(IReadOnlyList<InvoiceLineView> lines) =>
        [.. lines.GroupBy(l => l.ClassName).OrderBy(g => g.Key is null).Select(g => Group(g.Key, [.. g]))];

    private static InvoiceGroupView Group(string? className, IReadOnlyList<InvoiceLineView> lines) =>
        new(className, lines, lines.Sum(l => l.Amounts.Taxable + l.Amounts.Cgst + l.Amounts.Sgst + l.Amounts.Igst));

    private static List<InvoiceLineView> Merged(IReadOnlyList<InvoiceLineView> lines) =>
        [.. lines.GroupBy(l => (l.Item.Id, l.Price.VersionId, l.Price.Rate, l.ItemDiscount)).Select(Merge)];

    private static InvoiceLineView Merge(IGrouping<(Guid, Guid, decimal, Discount), InvoiceLineView> group)
    {
        var first = group.First();
        return first with
        {
            ClassName = null,
            Quantity = group.Sum(l => l.Quantity),
            Amounts = new SaleLineAmounts(
                group.Sum(l => l.Amounts.Gross), group.Sum(l => l.Amounts.ItemDiscount), group.Sum(l => l.Amounts.LineAmount),
                group.Sum(l => l.Amounts.TotalDiscountShare), group.Sum(l => l.Amounts.Taxable), group.Sum(l => l.Amounts.Cgst),
                group.Sum(l => l.Amounts.Sgst), group.Sum(l => l.Amounts.Igst))
        };
    }
}
