using BookInventory.Domain.Core;

namespace BookInventory.Domain.Sales;

/// <summary>
/// Spec sales R26: the layout a school's invoices open in (one row per school, made on the first change; until then
/// <see cref="Default"/>). It is a view choice only: an invoice's data, number and totals never change with it.
/// </summary>
public class SchoolInvoiceLayout : CompanyOwnedRoot
{
    public const InvoiceLayout Default = InvoiceLayout.BookList;

    private SchoolInvoiceLayout()
    {
    }

    public Guid SchoolId { get; private set; }

    public InvoiceLayout Layout { get; private set; }

    public static SchoolInvoiceLayout For(Guid schoolId, InvoiceLayout layout) => new() { SchoolId = schoolId, Layout = layout };

    public void Choose(InvoiceLayout layout) => Layout = layout;
}
