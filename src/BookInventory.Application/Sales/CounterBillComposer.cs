using BookInventory.Application.Item;
using BookInventory.Application.Price;
using BookInventory.Application.School;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.Domain.Item;
using BookInventory.Domain.Sales;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Application.Sales;

/// <summary>
/// R42-R45: turns a counter bill request into the domain's input, asking each module through its capability: the school (school R16),
/// the company's printed identity (company R17-R20), the student (R40, R41) and the items and their price versions through the
/// order composer, so an item or price refusal and the item-discount setting (R36) are the one rule of orders and invoices. The
/// price of a line is the version's MRP (Q25). Every value rule is the aggregate's.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class CounterBillComposer(
    PriceCompanyGuard companyGuard, ISchoolDirectory schools, InvoiceParties parties, OrderComposer orders, IItemDirectory items, StudentRecordService students)
{
    /// <summary>P20: a write needs a company in the session; a Super Admin who has not entered one is told so before anything is looked up.</summary>
    public void RequireCompany() => companyGuard.Require();

    public async Task<CounterBillInput> ComposeAsync(Counter counter, MakeCounterBillRequest request, CounterBillStamp stamp, CancellationToken cancellationToken)
    {
        var school = await schools.GetAsync(counter.SchoolId, cancellationToken) ?? throw new ValidationFailureException(CounterMessages.SchoolNotFound);
        var company = await parties.CompanySnapshotAsync(cancellationToken);
        var student = await students.ForBillAsync(counter.SchoolId, request.StudentId, request.Student, cancellationToken);
        var isGst = request.IsGstApplied ?? school.IsGstDefault;
        return new CounterBillInput
        {
            Number = stamp.Number,
            BillDate = stamp.BillDate,
            Today = stamp.Today,
            CounterId = counter.Id,
            Session = counter.Session,
            School = new SaleSchool(school.Id, school.Code, school.Name, school.State, school.Address),
            SchoolGstin = school.Gstin,
            Student = new StudentSnapshot(student.Id, student.Name, student.ClassName, student.Section, student.RollNumber, student.ParentMobile),
            CompanySnapshot = company,
            IsGstApplied = isGst,
            GstMode = SaleMoney.GstModeOf(isGst, company.State, school.State),
            TotalDiscount = request.TotalDiscount?.ToDiscount() ?? Discount.None,
            Lines = await LinesAsync(counter.SchoolId, stamp.BillDate, request.Lines ?? [], cancellationToken),
            Payments = [.. (request.Payments ?? []).Select(p => p.ToInput())],
            Remark = request.Remark
        };
    }

    /// <summary>R43: the same item and price version twice is one line with the quantities added; each line is priced at its version's MRP.</summary>
    private async Task<IReadOnlyList<CounterLineInput>> LinesAsync(
        Guid schoolId, DateOnly billDate, IReadOnlyList<CounterBillLineRequest> lines, CancellationToken cancellationToken)
    {
        var merged = lines
            .GroupBy(l => (l.ItemId, l.PriceVersionId))
            .Select(g => new SchoolOrderLineRequest(g.Key.ItemId, g.Key.PriceVersionId, g.Sum(l => l.Quantity ?? 0), g.First().ItemDiscount))
            .ToList();
        var composed = await orders.LinesAsync(new OrderLinesRequest(schoolId, billDate, merged), cancellationToken);
        var references = await items.SaleReferencesAsync([.. composed.Select(l => l.Item.Id)], cancellationToken);
        return [.. composed.Select(l => new CounterLineInput(l.Item, references.GetValueOrDefault(l.Item.Id)?.Category ?? ItemCategory.Other, l.Price, l.Quantity, l.ItemDiscount))];
    }
}
