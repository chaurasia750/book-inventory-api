using System.ComponentModel.DataAnnotations;
using BookInventory.Domain.Sales;

namespace BookInventory.Application.Sales;

/// <summary>R26: the layout a school's invoices open in.</summary>
public sealed record InvoiceLayoutResponse(Guid SchoolId, InvoiceLayout Layout);

/// <summary>R26: the Owner's choice of a school's default invoice layout.</summary>
public sealed record ChangeInvoiceLayoutRequest([param: Required] InvoiceLayout? Layout);

/// <summary>R26: the lines of an invoice of one class (<c>null</c>: general), or all merged lines of a book list, with their subtotal.</summary>
public sealed record InvoiceGroupView(string? ClassName, IReadOnlyList<InvoiceLineView> Lines, decimal Subtotal);
