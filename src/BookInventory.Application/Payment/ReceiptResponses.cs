using BookInventory.Application.School;
using BookInventory.Domain.Payment;

namespace BookInventory.Application.Payment;

/// <summary>R16: the one place a receipt becomes its answers (a DTO, never the entity).</summary>
public static class ReceiptResponses
{
    public static ReceiptSummary Summary(Receipt receipt, SchoolReference? school) =>
        new(receipt.Id, receipt.Number, receipt.ReceiptDate, receipt.SchoolId, school?.Code, school?.Name, receipt.TotalAmount, receipt.Status);

    public static ReceiptDetail Detail(Receipt receipt, SchoolReference? school, IReadOnlyDictionary<Guid, string> invoiceNumbers) =>
        new(
            Summary(receipt, school),
            receipt.Remark,
            receipt.AllocatedAmount,
            receipt.AdvanceAmount,
            [.. receipt.Modes.Select(m => new ReceiptModeDto(m.Id, m.ModeType, m.Amount, m.Reference, m.ChequeNumber, m.ChequeDate, m.BankName, m.ChequeStatus))],
            [
                .. receipt.Allocations
                    .GroupBy(a => a.InvoiceId)
                    .Select(g => new ReceiptAllocationDto(g.Key, invoiceNumbers.GetValueOrDefault(g.Key), g.Sum(a => a.Amount)))
                    .Where(a => a.Amount != 0m)
            ]);
}
