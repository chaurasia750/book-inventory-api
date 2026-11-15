namespace BookInventory.Domain.Payment;

/// <summary>Spec payment R6: an amount applied to one invoice.</summary>
public sealed record AllocationInput(Guid InvoiceId, decimal Amount);

/// <summary>Spec payment R3: an open invoice of a school with what is still due on it; lists of these are oldest invoice first.</summary>
public sealed record OpenInvoice(Guid InvoiceId, decimal Due);
