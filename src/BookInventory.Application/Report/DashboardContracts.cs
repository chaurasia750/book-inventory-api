using BookInventory.Application.Payment;
using BookInventory.Application.Sales;

namespace BookInventory.Application.Report;

/// <summary>R13: one part of the dashboard: its figures, or the code of the error when that part could not be read (the others still show).</summary>
public sealed record DashboardPart<T>(T? Data, string? Error = null) where T : class;

/// <summary>R3: a session of the dropdown.</summary>
public sealed record SessionChoice(string Label, bool IsCurrent);

/// <summary>R4: what was received and by which way, the number of receipts.</summary>
public sealed record ReceivedPart(decimal Total, IReadOnlyList<ModeFigure> ByMode, int Receipts);

/// <summary>R6: what came in this window on invoices of earlier sessions, from how many schools, and what is still due on those invoices.</summary>
public sealed record EarlierSessionsPart(decimal Amount, int Schools, decimal StillDue);

/// <summary>R4-R6: the payments card (the Owner's).</summary>
public sealed record PaymentsPart(ReceivedPart Received, PendingChequeFigure PendingCheques, EarlierSessionsPart EarlierSessions);

/// <summary>R7: the orders and invoices card.</summary>
public sealed record OrdersPart(IReadOnlyList<OrderStatusFigure> ByStatus, int StillToInvoiceQuantity, int InvoicesLive, decimal InvoicesTotal, int InvoicesCancelled);

/// <summary>R8: the sales card: today, the session and the last days by day.</summary>
public sealed record SalesPart(SalesFigure Today, SalesFigure Session, IReadOnlyList<DayFigure> Days);

/// <summary>R8: the returns and refunds card (the Owner's).</summary>
public sealed record ReturnsPart(int CreditNotes, decimal CreditNotesTotal, int Refunds, decimal RefundsTotal, int VendorReturns, decimal VendorReturnsTotal);

/// <summary>R9: one party in a "largest" list.</summary>
public sealed record OwedRow(Guid PartyId, string Code, string Name, decimal Balance);

/// <summary>R9: how many parties and their total, with the largest.</summary>
public sealed record OwedGroup(int Count, decimal Total, IReadOnlyList<OwedRow> Largest);

/// <summary>R9: the schools that owe, the schools that hold an advance and the vendors the company owes (the Owner's).</summary>
public sealed record OwedPart(OwedGroup SchoolsOwing, OwedGroup SchoolsInAdvance, OwedGroup VendorsOwed);

/// <summary>R10: the stock card; the value and the number of priceless items are the Owner's (null for Staff).</summary>
public sealed record StockPart(int AtOrBelowZero, int BelowZero, int DamagedPieces, decimal? Value, int? ItemsWithoutPrice);

/// <summary>R13: the dashboard: the session, its window, the dropdown and the parts the caller may see (the others are null).</summary>
public sealed record DashboardResponse(
    string Session,
    DateOnly From,
    DateOnly To,
    DateOnly Today,
    IReadOnlyList<SessionChoice> Sessions,
    DashboardPart<PaymentsPart>? Payments,
    DashboardPart<OrdersPart> Orders,
    DashboardPart<SalesPart> Sales,
    DashboardPart<ReturnsPart>? Returns,
    DashboardPart<OwedPart>? Owed,
    DashboardPart<StockPart> Stock);

/// <summary>R2, R3: what the dashboard is asked for: the session label (empty: the current) and whether the caller sees the money parts.</summary>
public sealed record DashboardQuery(string? Session, bool SeesMoney);
