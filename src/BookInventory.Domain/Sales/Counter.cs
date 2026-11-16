using System.Text.RegularExpressions;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.Domain.Core;

namespace BookInventory.Domain.Sales;

/// <summary>
/// A school counter (spec sales R38, R39): a sale point of the company at one school for one session. A company-owned aggregate
/// root. The dates are information only (Q24): the counter never refuses a bill for its dates, only for being closed. The school
/// cannot change once a bill was made at it.
/// </summary>
public partial class Counter : CompanyOwnedRoot
{
    public const int NameMinLength = 2;
    public const int NameMaxLength = 150;
    public const int SessionMaxLength = 20;

    private Counter()
    {
        Name = string.Empty;
        Session = string.Empty;
    }

    public Guid SchoolId { get; private set; }

    public string Name { get; private set; }

    public string Session { get; private set; }

    public DateOnly StartDate { get; private set; }

    public DateOnly? EndDate { get; private set; }

    public CounterStatus Status { get; private set; }

    /// <summary>R39: set by the first bill made here; from then on the school is fixed.</summary>
    public bool HasBills { get; private set; }

    public bool IsActive => Status == CounterStatus.Active;

    public static Counter Open(CounterDetails details)
    {
        var counter = new Counter { Status = CounterStatus.Active };
        counter.Apply(details);
        return counter;
    }

    /// <summary>R38, R39: changes name, session and dates; the school only while no bill exists.</summary>
    public void Edit(CounterDetails details)
    {
        if (HasBills && details.SchoolId != SchoolId)
            throw new ConflictException(CounterMessages.CounterSchoolFixed);

        Apply(details);
    }

    public void Close()
    {
        if (Status == CounterStatus.Closed)
            throw new ConflictException(CounterMessages.CounterAlreadyClosed);

        Status = CounterStatus.Closed;
    }

    public void Reopen()
    {
        if (Status == CounterStatus.Active)
            throw new ConflictException(CounterMessages.CounterAlreadyOpen);

        Status = CounterStatus.Active;
    }

    /// <summary>R39: a bill may be made only at an open counter; making one fixes the school.</summary>
    public void RecordBill()
    {
        if (Status == CounterStatus.Closed)
            throw new ConflictException(CounterMessages.CounterClosed);

        HasBills = true;
    }

    private void Apply(CounterDetails details)
    {
        var name = SpaceRun().Replace(details.Name?.Trim() ?? string.Empty, " ");
        if (name.Length is < NameMinLength or > NameMaxLength)
            throw new ValidationFailureException(CounterMessages.InvalidCounterName);

        var session = details.Session?.Trim() ?? string.Empty;
        if (session.Length is 0 or > SessionMaxLength)
            throw new ValidationFailureException(CounterMessages.InvalidCounterSession);

        if (details.EndDate is { } end && end < details.StartDate)
            throw new ValidationFailureException(CounterMessages.InvalidCounterDates);

        SchoolId = details.SchoolId;
        Name = name;
        Session = session;
        StartDate = details.StartDate;
        EndDate = details.EndDate;
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex SpaceRun();
}
