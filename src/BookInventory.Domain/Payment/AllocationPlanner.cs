using BookInventory.BuildingBlocks.Domain.Exceptions;

namespace BookInventory.Domain.Payment;

/// <summary>
/// Spec payment R6: how a payment is applied to a school's open invoices. By default the total goes to the oldest invoice first,
/// each up to its due; a hand-made list is checked (open invoices of the school, each amount above zero and at most its due, the sum
/// at most the total). What is not applied is the advance.
/// </summary>
public static class AllocationPlanner
{
    public static IReadOnlyList<AllocationInput> Plan(decimal total, IReadOnlyList<OpenInvoice> open, IReadOnlyList<AllocationInput>? requested)
    {
        if (requested is not { Count: > 0 })
            return OldestFirst(total, open);

        var perInvoice = requested
            .GroupBy(a => a.InvoiceId)
            .Select(g => new AllocationInput(g.Key, g.Sum(a => PaymentMoney.Require(a.Amount, PaymentMessages.InvalidAllocation))))
            .ToList();
        foreach (var allocation in perInvoice)
        {
            var invoice = open.FirstOrDefault(o => o.InvoiceId == allocation.InvoiceId)
                          ?? throw new ValidationFailureException(PaymentMessages.InvoiceNotOpen) { Subject = allocation.InvoiceId.ToString() };
            if (allocation.Amount > invoice.Due)
                throw new ValidationFailureException(PaymentMessages.InvalidAllocation) { Subject = allocation.InvoiceId.ToString() };
        }

        return perInvoice.Sum(a => a.Amount) <= total ? perInvoice : throw new ValidationFailureException(PaymentMessages.InvalidAllocation);
    }

    private static List<AllocationInput> OldestFirst(decimal total, IReadOnlyList<OpenInvoice> open)
    {
        var plan = new List<AllocationInput>();
        var left = total;
        foreach (var invoice in open.Where(o => o.Due > 0m))
        {
            if (left <= 0m)
                break;

            var take = Math.Min(left, invoice.Due);
            plan.Add(new AllocationInput(invoice.InvoiceId, take));
            left -= take;
        }

        return plan;
    }
}
