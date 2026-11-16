using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.Domain.Core;
using BookInventory.Domain.Sales;

namespace BookInventory.Domain.Purchase;

/// <summary>
/// Spec purchase R2-R8, R10: items sent back to one vendor. It is complete when made (the snapshots of R10, the amounts of R5) and
/// never edited; the Owner cancels it with a reason and it keeps its number. Its ledger posting is
/// <see cref="VendorReturnLedgerEntry"/>; the rounding is the one rule of sales R7 (<see cref="SaleMoney"/>), written once.
/// </summary>
public class VendorReturn : CompanyOwnedRoot
{
    public const int MaxLines = 500;
    public const int RemarkMaxLength = 500;
    public const int ReasonMinLength = 5;
    public const int ReasonMaxLength = 500;

    /// <summary>R7: the stock and ledger document type of a vendor return.</summary>
    public const string DocumentType = "VENDOR_RETURN";

    private readonly List<VendorReturnLine> _lines = [];

    private VendorReturn()
    {
        Number = string.Empty;
        FinancialYear = string.Empty;
        VendorCode = string.Empty;
        VendorName = string.Empty;
    }

    public string Number { get; private set; }

    public string FinancialYear { get; private set; }

    public DateOnly ReturnDate { get; private set; }

    public Guid VendorId { get; private set; }

    public string VendorCode { get; private set; }

    public string VendorName { get; private set; }

    /// <summary>R10: the company's printed identity when the vendor return was made.</summary>
    public CompanyInvoiceSnapshot CompanySnapshot { get; private set; } = CompanyInvoiceSnapshot.Empty;

    public string? Remark { get; private set; }

    public decimal SubtotalAmount { get; private set; }

    public decimal RoundOffAmount { get; private set; }

    public decimal GrandTotalAmount { get; private set; }

    public VendorReturnStatus Status { get; private set; }

    public string? CancelReason { get; private set; }

    public DateOnly? CancelledOn { get; private set; }

    public IReadOnlyList<VendorReturnLine> Lines => _lines.OrderBy(l => l.SortOrder).ToList();

    /// <summary>R3 (FR-CMP-04): the code series of a financial year, <c>DN/2026-27/</c>.</summary>
    public static string NumberSeries(string financialYear) => $"DN/{financialYear}/";

    /// <summary>R3: <c>DN/2026-27/0001</c>.</summary>
    public static string NumberOf(string financialYear, int number) => $"{NumberSeries(financialYear)}{number:D4}";

    /// <summary>R2-R5: the vendor return; refused for a date in the future, no or too many lines, a bad rate, quantity or source, or a total not above zero.</summary>
    public static VendorReturn Make(VendorReturnInput input)
    {
        Check(input);
        var amounts = input.Lines.Select(l => SaleMoney.Round2(l.Rate * l.Quantity)).ToList();
        var subtotal = amounts.Sum();
        var total = SaleMoney.RoundRupee(subtotal);
        if (total <= 0m)
            throw new ValidationFailureException(PurchaseMessages.TotalNotPositive);

        var vendorReturn = new VendorReturn
        {
            Number = input.Number,
            FinancialYear = Finance.FinancialYear.Of(input.ReturnDate),
            ReturnDate = input.ReturnDate,
            VendorId = input.Vendor.Id,
            VendorCode = input.Vendor.Code,
            VendorName = input.Vendor.Name,
            CompanySnapshot = input.CompanySnapshot,
            Remark = string.IsNullOrWhiteSpace(input.Remark) ? null : input.Remark.Trim(),
            SubtotalAmount = subtotal,
            RoundOffAmount = total - subtotal,
            GrandTotalAmount = total,
            Status = VendorReturnStatus.Active
        };
        vendorReturn._lines.AddRange(input.Lines.Select((l, i) => VendorReturnLine.Of(vendorReturn.Id, i + 1, l, amounts[i])));
        vendorReturn.AddDomainEvent(new VendorReturnMade(vendorReturn.Id, vendorReturn.VendorId, vendorReturn.Number, total));
        return vendorReturn;
    }

    /// <summary>R8: the Owner cancels a live vendor return with a reason (5-500 characters); the number stays.</summary>
    public void Cancel(string reason, DateOnly today)
    {
        if (Status == VendorReturnStatus.Cancelled)
            throw new ConflictException(PurchaseMessages.AlreadyCancelled);

        var trimmed = reason?.Trim() ?? string.Empty;
        if (trimmed.Length is < ReasonMinLength or > ReasonMaxLength)
            throw new ValidationFailureException(PurchaseMessages.InvalidReason);

        Status = VendorReturnStatus.Cancelled;
        CancelReason = trimmed;
        CancelledOn = today;
        AddDomainEvent(new VendorReturnCancelled(Id, VendorId, Number));
    }

    private static void Check(VendorReturnInput input)
    {
        if (input.ReturnDate > input.Today)
            throw new ValidationFailureException(PurchaseMessages.InvalidDate);
        if (input.Lines.Count == 0)
            throw new ValidationFailureException(PurchaseMessages.NoLines);
        if (input.Lines.Count > MaxLines)
            throw new ValidationFailureException(PurchaseMessages.TooManyLines);
        if (input.Remark?.Trim().Length > RemarkMaxLength)
            throw new ValidationFailureException(PurchaseMessages.InvalidText);

        foreach (var line in input.Lines)
            CheckLine(line);
    }

    private static void CheckLine(VendorReturnLineInput line)
    {
        if (line.Rate <= 0m || line.Rate > SaleMoney.MaxAmount || !SaleMoney.HasAtMostTwoDecimals(line.Rate))
            throw new ValidationFailureException(PurchaseMessages.InvalidRate);
        if (line.Quantity is < 1 or > SchoolOrder.MaxQuantity)
            throw new ValidationFailureException(PurchaseMessages.InvalidQuantity);
        if (!Enum.IsDefined(line.Source))
            throw new ValidationFailureException(PurchaseMessages.InvalidSource);
    }
}
