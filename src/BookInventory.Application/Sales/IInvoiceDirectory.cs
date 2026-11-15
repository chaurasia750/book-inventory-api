using BookInventory.Domain.Sales;

namespace BookInventory.Application.Sales;

/// <summary>R37: what another module may know about an invoice: its id, number, date, session, school, grand total and status.</summary>
public sealed record InvoiceReference(Guid Id, string Number, DateOnly InvoiceDate, string Session, Guid SchoolId, decimal GrandTotal, InvoiceStatus Status);

/// <summary>
/// R37: the read capability other modules (the payment module first) use to read invoices, never by joining the sales tables. It
/// answers only for the company of the session (platform P19), returns plain values and never the aggregate, and changes nothing.
/// </summary>
public interface IInvoiceDirectory
{
    /// <summary>The school's <c>ISSUED</c> invoices (not cancelled), oldest first (date, then number).</summary>
    Task<IReadOnlyList<InvoiceReference>> IssuedOfSchoolAsync(Guid schoolId, CancellationToken cancellationToken = default);

    /// <summary>The given invoices by id, cancelled ones included (marked by their status); an unknown id or another company's is left out.</summary>
    Task<IReadOnlyDictionary<Guid, InvoiceReference>> ReferencesAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default);
}
