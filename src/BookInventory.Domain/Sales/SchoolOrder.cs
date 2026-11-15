using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.Domain.Core;

namespace BookInventory.Domain.Sales;

/// <summary>
/// Spec sales R2-R7, R12: a school's order: a draft that changes freely, then a confirmed order whose lines no longer change and
/// from which invoices take quantities, closed when all is invoiced or when the Owner closes it. Its amounts always come from
/// <see cref="SaleCalculation"/> (D2), recomputed whenever its lines change.
/// </summary>
public class SchoolOrder : CompanyOwnedRoot
{
    public const int MaxLines = 500;
    public const int MaxQuantity = 100_000;
    public const int RemarkMaxLength = 500;
    public const int SessionMaxLength = 20;
    public const int ReasonMinLength = 5;
    public const int ReasonMaxLength = 500;

    private readonly List<SchoolOrderLine> _lines = [];

    private SchoolOrder()
    {
        Number = string.Empty;
        SchoolCode = string.Empty;
        SchoolName = string.Empty;
        Session = string.Empty;
    }

    public string Number { get; private set; }

    public Guid SchoolId { get; private set; }

    public string SchoolCode { get; private set; }

    public string SchoolName { get; private set; }

    public string? SchoolState { get; private set; }

    public DateOnly OrderDate { get; private set; }

    public string Session { get; private set; }

    public bool IsGstApplied { get; private set; }

    public GstMode GstMode { get; private set; }

    /// <summary>R34: Cash or Credit; starts from the school's default.</summary>
    public PaymentType PaymentType { get; private set; }

    public DiscountKind TotalDiscountKind { get; private set; }

    public decimal TotalDiscountValue { get; private set; }

    public string? Remark { get; private set; }

    public SchoolOrderStatus Status { get; private set; }

    public string? CloseReason { get; private set; }

    public SaleTotals Totals { get; private set; } = SaleTotals.Zero;

    /// <summary>R8: the part of a rupee total discount the live invoices already applied.</summary>
    public decimal TotalDiscountGiven { get; private set; }

    /// <summary>The live lines in their order (removed draft lines stay soft-deleted in the backing field).</summary>
    public IReadOnlyList<SchoolOrderLine> Lines => _lines.Where(l => !l.IsDeleted).OrderBy(l => l.SortOrder).ToList();

    public Discount TotalDiscount => new(TotalDiscountKind, TotalDiscountValue);

    /// <summary>R2 (owner's answer Q1): the code series of a session, <c>ORD/2026-27/</c>.</summary>
    public static string NumberSeries(string session) => $"ORD/{session}/";

    /// <summary>R2: <c>ORD/2026-27/0001</c>.</summary>
    public static string NumberOf(string session, int number) => $"{NumberSeries(session)}{number:D4}";

    /// <summary>R2, R6: a new draft.</summary>
    public static SchoolOrder Draft(string number, OrderHeader header, IReadOnlyList<OrderLineInput> lines, DateOnly today)
    {
        var order = new SchoolOrder { Number = number, Status = SchoolOrderStatus.Draft };
        order.Apply(header, lines, today);
        return order;
    }

    /// <summary>R6: a draft's values and lines are replaced; a new number only when its session changed.</summary>
    public void Replace(OrderHeader header, IReadOnlyList<OrderLineInput> lines, DateOnly today, string? newNumber = null)
    {
        RequireDraft();
        if (newNumber is not null)
            Number = newNumber;
        Apply(header, lines, today);
    }

    /// <summary>
    /// R20: a confirmed order changes above what is invoiced: lines are edited (<see cref="OrderLineInput.LineId"/>), added (no id) or
    /// left out (deleted, only when nothing is invoiced on them); the total discount and the GST choice change only while nothing is
    /// invoiced; the remark always. When nothing remains to deliver the order closes.
    /// </summary>
    public void Amend(OrderHeader header, IReadOnlyList<OrderLineInput> lines)
    {
        if (Status != SchoolOrderStatus.Confirmed)
            throw new ConflictException(SalesMessages.NotConfirmed);
        OrderRules.RequireAmendable(header, SchoolId);
        ChangeTotalsOf(header);
        Remark = string.IsNullOrWhiteSpace(header.Remark) ? null : header.Remark.Trim();
        var kept = OrderRules.Edit(Lines, lines.Where(l => l.LineId is not null).ToList());
        foreach (var gone in Lines.Where(l => !kept.Contains(l.Id)))
            OrderRules.Withdraw(gone);

        AddMerged(lines.Where(l => l.LineId is null).ToList());
        OrderRules.RequireNoRepeats(Lines);
        if (Lines.Count == 0)
            throw new ValidationFailureException(SalesMessages.NoLines);

        Recalculate();
        CloseWhenNothingRemains();
    }

    /// <summary>R22, R23: lines are added to a draft or a confirmed order; the same item, version and class raises that line (R21).</summary>
    public void AddLines(IReadOnlyList<OrderLineInput> additions)
    {
        if (Status is not (SchoolOrderStatus.Draft or SchoolOrderStatus.Confirmed))
            throw new ConflictException(SalesMessages.NotOpen);

        AddMerged(additions);
        Recalculate();
    }

    private bool HasInvoicedAnything => TotalDiscountGiven > 0m || Lines.Any(l => l.InvoicedQuantity > 0);

    private void ChangeTotalsOf(OrderHeader header)
    {
        if (header.IsGstApplied == IsGstApplied && header.TotalDiscount == TotalDiscount && header.PaymentType == PaymentType)
            return;
        if (HasInvoicedAnything)
            throw new ConflictException(SalesMessages.TotalsLocked);

        IsGstApplied = header.IsGstApplied;
        GstMode = header.GstMode;
        PaymentType = header.PaymentType;
        TotalDiscountKind = header.TotalDiscount.Kind;
        TotalDiscountValue = header.TotalDiscount.Value;
    }

    private void AddMerged(IReadOnlyList<OrderLineInput> additions)
    {
        foreach (var addition in OrderRules.Merge(additions))
        {
            var same = Lines.FirstOrDefault(l => l.Matches(addition));
            if (same is null)
                _lines.Add(SchoolOrderLine.Of(Id, _lines.Count == 0 ? 1 : _lines.Max(l => l.SortOrder) + 1, addition));
            else
                same.Raise(addition.Quantity);
        }

        if (Lines.Count > MaxLines)
            throw new ValidationFailureException(SalesMessages.TooManyLines);
    }

    private void CloseWhenNothingRemains()
    {
        if (Lines.Any(l => l.RemainingQuantity > 0))
            return;

        Status = SchoolOrderStatus.Closed;
        AddDomainEvent(new SchoolOrderClosed(Id, SchoolId, Number));
    }

    /// <summary>R6: only a draft is deleted.</summary>
    public void RequireDraft()
    {
        if (Status != SchoolOrderStatus.Draft)
            throw new ConflictException(SalesMessages.NotDraft);
    }

    /// <summary>R6: a draft with at least one line becomes a confirmed order.</summary>
    public void Confirm()
    {
        RequireDraft();
        if (Lines.Count == 0)
            throw new ValidationFailureException(SalesMessages.NoLines);

        Status = SchoolOrderStatus.Confirmed;
        AddDomainEvent(new SchoolOrderConfirmed(Id, SchoolId, Number));
    }

    /// <summary>R6 (owner's answer Q14): the Owner closes a confirmed order with a reason; nothing more is invoiced from it.</summary>
    public void Close(string reason)
    {
        if (Status != SchoolOrderStatus.Confirmed)
            throw new ConflictException(SalesMessages.NotConfirmed);
        var trimmed = reason?.Trim() ?? string.Empty;
        if (trimmed.Length is < ReasonMinLength or > ReasonMaxLength)
            throw new ValidationFailureException(SalesMessages.InvalidReason);

        CloseReason = trimmed;
        Status = SchoolOrderStatus.Closed;
        AddDomainEvent(new SchoolOrderClosed(Id, SchoolId, Number));
    }

    /// <summary>
    /// R8 (owner's answers Q2, Q10): an invoice takes quantities from a confirmed order (at least one, never more than remains) with
    /// its share of the rupee discounts; the delivery that empties the order takes what is left of them, and the order closes.
    /// </summary>
    public OrderDelivery Deliver(IReadOnlyDictionary<Guid, int> quantities)
    {
        if (Status != SchoolOrderStatus.Confirmed)
            throw new ConflictException(SalesMessages.NotConfirmed);
        var taken = quantities.Where(q => q.Value != 0).ToList();
        if (taken.Count == 0)
            throw new ValidationFailureException(SalesMessages.NothingToInvoice);

        var lines = Lines;
        var delivery = new List<DeliveryLine>();
        foreach (var (lineId, quantity) in taken)
        {
            var line = lines.FirstOrDefault(l => l.Id == lineId) ?? throw new ValidationFailureException(SalesMessages.NothingToInvoice);
            if (quantity < 0 || quantity > line.RemainingQuantity)
                throw new ValidationFailureException(SalesMessages.QuantityAboveRemaining);
            delivery.Add(new DeliveryLine(line.Id, line.Item, line.Price, quantity, line.DiscountFor(quantity), line.ClassName));
        }

        var emptiesOrder = lines.All(l => l.RemainingQuantity == delivery.Where(d => d.OrderLineId == l.Id).Sum(d => d.Quantity));
        var result = new OrderDelivery(delivery, TotalDiscountFor(delivery, emptiesOrder));
        Take(result, sign: 1);
        if (emptiesOrder)
        {
            Status = SchoolOrderStatus.Closed;
            AddDomainEvent(new SchoolOrderClosed(Id, SchoolId, Number));
        }

        return result;
    }

    /// <summary>R12: a cancelled invoice gives back its quantities and rupee discounts; an order closed only because all was invoiced opens again.</summary>
    public void GiveBack(OrderDelivery delivery)
    {
        Take(delivery, sign: -1);
        if (Status == SchoolOrderStatus.Closed && CloseReason is null && Lines.Any(l => l.RemainingQuantity > 0))
            Status = SchoolOrderStatus.Confirmed;
    }

    /// <summary>R8: a percentage as it is; a rupee total discount by the delivery's share of the order subtotal, the last delivery taking the rest.</summary>
    private Discount TotalDiscountFor(IReadOnlyList<DeliveryLine> delivery, bool emptiesOrder)
    {
        if (TotalDiscountKind != DiscountKind.Amount)
            return TotalDiscount;
        if (emptiesOrder)
            return new Discount(DiscountKind.Amount, TotalDiscountValue - TotalDiscountGiven);

        var subtotal = delivery.Sum(d => SaleMoney.Round2(d.Price.Rate * d.Quantity) - d.ItemDiscount.AmountOn(SaleMoney.Round2(d.Price.Rate * d.Quantity)));
        var share = Totals.Subtotal == 0m ? 0m : SaleMoney.Round2(TotalDiscountValue * subtotal / Totals.Subtotal);
        return new Discount(DiscountKind.Amount, Math.Min(share, TotalDiscountValue - TotalDiscountGiven));
    }

    private void Take(OrderDelivery delivery, int sign)
    {
        foreach (var d in delivery.Lines)
        {
            var itemAmount = d.ItemDiscount.Kind == DiscountKind.Amount ? d.ItemDiscount.Value : 0m;
            Lines.FirstOrDefault(l => l.Id == d.OrderLineId)?.ChangeInvoiced(sign * d.Quantity, sign * itemAmount);
        }

        if (delivery.TotalDiscount.Kind == DiscountKind.Amount)
            TotalDiscountGiven = Math.Max(0m, TotalDiscountGiven + (sign * delivery.TotalDiscount.Value));
    }

    private void Apply(OrderHeader header, IReadOnlyList<OrderLineInput> lines, DateOnly today)
    {
        OrderRules.RequireHeader(header, today);
        var merged = OrderRules.Merge(lines);

        SchoolId = header.School.Id;
        SchoolCode = header.School.Code;
        SchoolName = header.School.Name;
        SchoolState = header.School.State;
        OrderDate = header.OrderDate;
        Session = header.Session.Trim();
        IsGstApplied = header.IsGstApplied;
        GstMode = header.GstMode;
        PaymentType = header.PaymentType;
        TotalDiscountKind = header.TotalDiscount.Kind;
        TotalDiscountValue = header.TotalDiscount.Value;
        Remark = string.IsNullOrWhiteSpace(header.Remark) ? null : header.Remark.Trim();

        foreach (var old in _lines.Where(l => !l.IsDeleted))
            old.MarkDeleted();
        _lines.AddRange(merged.Select((l, i) => SchoolOrderLine.Of(Id, i + 1, l)));
        Recalculate();
    }

    private void Recalculate()
    {
        var live = Lines;
        var amounts = SaleCalculation.Compute([.. live.Select(l => l.CalculationInput)], TotalDiscount, GstMode);
        for (var i = 0; i < live.Count; i++)
            live[i].SetAmounts(amounts.Lines[i]);
        Totals = amounts.Totals;
    }
}
