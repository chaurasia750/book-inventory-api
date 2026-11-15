using BookInventory.Application.Company;
using BookInventory.Application.Item;
using BookInventory.Application.Price;
using BookInventory.Application.School;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.Domain.Sales;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Application.Sales;

/// <summary>
/// R1-R4, R13: turns an order request into the domain's header and lines, asking each module through its capability: the school
/// (school R16) and its session (school R18), the company state for GST (company R16), the items (item R16) and their price
/// versions (price R18). Every value rule is the aggregate's; this class only finds things and refuses what does not exist.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class OrderComposer(
    ISchoolDirectory schools, ISchoolSessions sessions, ICompanyTaxProfile company, IItemDirectory items, IPriceDirectory prices)
{
    public async Task<OrderHeader> HeaderAsync(SaveSchoolOrderRequest request, DateOnly orderDate, CancellationToken cancellationToken)
    {
        var school = await schools.GetAsync(request.SchoolId!.Value, cancellationToken) ?? throw new ValidationFailureException(SalesMessages.SchoolNotFound);
        if (!school.IsActive)
            throw new ValidationFailureException(SalesMessages.SchoolInactive);

        var choice = await sessions.SessionOfAsync(school.Id, orderDate, cancellationToken) ?? throw new ValidationFailureException(SalesMessages.SchoolNotFound);
        var session = string.IsNullOrWhiteSpace(request.Session) ? choice.Session : request.Session.Trim();
        if (session != choice.Session && session != choice.Previous && session != choice.Next)
            throw new ValidationFailureException(SalesMessages.InvalidSession);

        var isGst = request.IsGstApplied ?? school.IsGstDefault;
        var tax = await company.CurrentAsync(cancellationToken);
        return new OrderHeader
        {
            School = new SaleSchool(school.Id, school.Code, school.Name, school.State),
            OrderDate = orderDate,
            Session = session,
            IsGstApplied = isGst,
            GstMode = SaleMoney.GstModeOf(isGst, tax?.State, school.State),
            TotalDiscount = request.TotalDiscount?.ToDiscount() ?? Discount.None,
            Remark = request.Remark
        };
    }

    public async Task<IReadOnlyList<OrderLineInput>> LinesAsync(IReadOnlyList<SchoolOrderLineRequest> lines, DateOnly orderDate, CancellationToken cancellationToken)
    {
        if (lines.Count == 0)
            return [];
        if (lines.Count > SchoolOrder.MaxLines)
            throw new ValidationFailureException(SalesMessages.TooManyLines);

        var itemIds = lines.Select(l => l.ItemId!.Value).Distinct().ToList();
        var references = await items.SaleReferencesAsync(itemIds, cancellationToken);
        var versions = await prices.VersionsOfItemsAsync(itemIds, orderDate, cancellationToken);
        return [.. lines.Select(l => LineOf(l, references, versions))];
    }

    private static OrderLineInput LineOf(
        SchoolOrderLineRequest line, IReadOnlyDictionary<Guid, ItemSaleReference> references, IReadOnlyDictionary<Guid, ItemPriceVersions> versions)
    {
        var reference = references.GetValueOrDefault(line.ItemId!.Value) ?? throw new ValidationFailureException(SalesMessages.ItemNotFound);
        if (!reference.Item.IsActive)
            throw new ValidationFailureException(SalesMessages.ItemInactive);

        var itemVersions = versions[reference.Item.Id];
        if (itemVersions.Versions.Count == 0)
            throw new ValidationFailureException(SalesMessages.NoPrice);
        var versionId = line.PriceVersionId ?? itemVersions.DefaultVersionId ?? itemVersions.Versions[0].Id;
        var version = itemVersions.Versions.FirstOrDefault(v => v.Id == versionId) ?? throw new ValidationFailureException(SalesMessages.InvalidPriceVersion);

        return new OrderLineInput(
            new SaleItem(reference.Item.Id, reference.Item.Code, reference.Item.Name, reference.HsnCode, reference.GstRatePercent),
            new SalePrice(version.Id, version.VersionName, version.MrpAmount, version.SaleAmount),
            line.Quantity!.Value,
            line.ItemDiscount?.ToDiscount() ?? Discount.None);
    }
}
