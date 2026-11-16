using BookInventory.Domain.Sales;

namespace BookInventory.Application.Sales;

/// <summary>The human text of every <see cref="CounterMessages"/> code; the code travels, the text is looked up (R55 of iam).</summary>
public static class CounterMessageCatalog
{
    public static IReadOnlyDictionary<string, string> Messages { get; } =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [CounterMessages.CounterNotFound] = "The counter could not be found.",
            [CounterMessages.InvalidCounterName] = "The counter name must be 2 to 150 characters.",
            [CounterMessages.InvalidCounterDates] = "The end date cannot be before the start date.",
            [CounterMessages.InvalidCounterSession] = "Choose the school's session on the start date, or the one before or after it.",
            [CounterMessages.SchoolNotFound] = "The school could not be found.",
            [CounterMessages.SchoolNotCounter] = "Choose an active school that sells at the counter.",
            [CounterMessages.CounterSchoolFixed] = "The school of a counter cannot change after a bill was made at it.",
            [CounterMessages.CounterClosed] = "This counter is closed. Open it again to make a bill.",
            [CounterMessages.CounterAlreadyClosed] = "The counter is already closed.",
            [CounterMessages.CounterAlreadyOpen] = "The counter is already open.",
            [CounterMessages.StudentNotFound] = "The student could not be found.",
            [CounterMessages.InvalidStudentName] = "The student name must be 2 to 150 characters.",
            [CounterMessages.InvalidStudentField] = "A student detail is too long.",
            [CounterMessages.InvalidStudentClass] = "Choose one of the classes of the school.",
            [CounterMessages.InvalidStudentMobile] = "The mobile number must have 10 to 15 digits.",
            [CounterMessages.BillNotFound] = "The counter bill could not be found.",
            [CounterMessages.InvalidBillDate] = "The bill date cannot be in the future.",
            [CounterMessages.BillTooManyLines] = "A counter bill can have at most 200 lines.",
            [CounterMessages.BillNoLines] = "Add at least one item to the bill.",
            [CounterMessages.InvalidPayment] = "Each payment needs Cash or UPI and an amount above zero with at most two decimals.",
            [CounterMessages.PaymentMismatch] = "The Cash and UPI amounts must add up to the bill total, with at most one payment of each kind.",
            [CounterMessages.BillAlreadyCancelled] = "The bill is already cancelled."
        };
}
