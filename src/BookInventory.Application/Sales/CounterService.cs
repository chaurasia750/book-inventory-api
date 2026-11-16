using BookInventory.Application.Price;
using BookInventory.Application.School;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.Domain.School;
using BookInventory.Domain.Sales;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Application.Sales;

/// <summary>
/// R38, R39: opens, edits, closes and reopens school counters. The school must be an active school of the company that sells at the
/// counter, and the session is the school's own on the start date (or the one before or after it), both read through the school
/// directory and sessions capabilities (school R16, R18). Every other rule is the aggregate's.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class CounterService(ICounterStore store, ISchoolDirectory schools, ISchoolSessions sessions, PriceCompanyGuard companyGuard)
{
    public async Task<CounterResponse> OpenAsync(SaveCounterRequest request, CancellationToken cancellationToken)
    {
        companyGuard.Require();
        var school = await RequireCounterSchoolAsync(request.SchoolId!.Value, cancellationToken);
        var counter = Counter.Open(await DetailsAsync(request, cancellationToken));
        await store.AddCounterAsync(counter, cancellationToken);
        return CounterResponses.Of(counter, school);
    }

    public async Task EditAsync(Guid id, SaveCounterRequest request, CancellationToken cancellationToken)
    {
        companyGuard.Require();
        var counter = await LoadAsync(id, cancellationToken);
        await RequireCounterSchoolAsync(request.SchoolId!.Value, cancellationToken);
        counter.Edit(await DetailsAsync(request, cancellationToken));
    }

    public async Task CloseAsync(Guid id, CancellationToken cancellationToken)
    {
        companyGuard.Require();
        (await LoadAsync(id, cancellationToken)).Close();
    }

    public async Task ReopenAsync(Guid id, CancellationToken cancellationToken)
    {
        companyGuard.Require();
        (await LoadAsync(id, cancellationToken)).Reopen();
    }

    private async Task<Counter> LoadAsync(Guid id, CancellationToken cancellationToken) =>
        await store.FindCounterForManagementAsync(id, cancellationToken) ?? throw new NotFoundException(CounterMessages.CounterNotFound);

    private async Task<SchoolReference> RequireCounterSchoolAsync(Guid schoolId, CancellationToken cancellationToken)
    {
        var school = await schools.GetAsync(schoolId, cancellationToken) ?? throw new ValidationFailureException(CounterMessages.SchoolNotFound);
        if (!school.IsActive || school.SaleType == SaleType.SchoolOrder)
            throw new ValidationFailureException(CounterMessages.SchoolNotCounter);

        return school;
    }

    /// <summary>R38: the session is the school's on the start date unless the Owner picked the one before or after it.</summary>
    private async Task<CounterDetails> DetailsAsync(SaveCounterRequest request, CancellationToken cancellationToken)
    {
        var start = request.StartDate!.Value;
        var choice = await sessions.SessionOfAsync(request.SchoolId!.Value, start, cancellationToken)
            ?? throw new ValidationFailureException(CounterMessages.SchoolNotFound);
        var session = string.IsNullOrWhiteSpace(request.Session) ? choice.Session : request.Session.Trim();
        if (session != choice.Session && session != choice.Previous && session != choice.Next)
            throw new ValidationFailureException(CounterMessages.InvalidCounterSession);

        return new CounterDetails(request.SchoolId.Value, request.Name, session, start, request.EndDate);
    }
}
