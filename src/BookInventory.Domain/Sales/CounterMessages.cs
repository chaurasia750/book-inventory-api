namespace BookInventory.Domain.Sales;

/// <summary>
/// Every message code of the school counter sale (spec sales R38-R52): counters, students and counter bills. The domain throws these
/// codes; the human text lives in <c>BookInventory.Application.Sales.CounterMessageCatalog</c>. Codes are stable API surface.
/// </summary>
public static class CounterMessages
{
    public const string CounterNotFound = "sales.counter.not_found";
    public const string InvalidCounterName = "sales.counter.invalid_name";
    public const string InvalidCounterDates = "sales.counter.invalid_dates";
    public const string InvalidCounterSession = "sales.counter.invalid_session";
    public const string SchoolNotFound = "sales.counter.school_not_found";
    public const string SchoolNotCounter = "sales.counter.school_not_counter";
    public const string CounterSchoolFixed = "sales.counter.school_fixed";
    public const string CounterClosed = "sales.counter.closed";
    public const string CounterAlreadyClosed = "sales.counter.already_closed";
    public const string CounterAlreadyOpen = "sales.counter.already_open";

    public const string StudentNotFound = "sales.student.not_found";
    public const string InvalidStudentName = "sales.student.invalid_name";
    public const string InvalidStudentField = "sales.student.invalid_field";
    public const string InvalidStudentClass = "sales.student.invalid_class";
    public const string InvalidStudentMobile = "sales.student.invalid_mobile";
}
