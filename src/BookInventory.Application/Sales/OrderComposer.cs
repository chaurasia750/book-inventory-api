using BookInventory.Application.Company;
using BookInventory.Application.Item;
using BookInventory.Application.Price;
using BookInventory.Application.School;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.Domain.School;
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

    /// <summary>R3, R25: every line or the first refusal, which names the line (item code and class).</summary>
    public async Task<IReadOnlyList<OrderLineInput>> LinesAsync(OrderLinesRequest request, CancellationToken cancellationToken) =>
        (await ComposeAsync(request, strict: true, cancellationToken)).Lines;

    /// <summary>R22, R23: the lines that can be made and, for the others, the reason each was left out.</summary>
    public Task<ComposedLines> TryLinesAsync(OrderLinesRequest request, CancellationToken cancellationToken) =>
        ComposeAsync(request, strict: false, cancellationToken);

    private async Task<ComposedLines> ComposeAsync(OrderLinesRequest request, bool strict, CancellationToken cancellationToken)
    {
        var lines = request.Lines;
        if (lines.Count == 0)
            return new ComposedLines([], []);
        if (lines.Count > SchoolOrder.MaxLines)
            throw new ValidationFailureException(SalesMessages.TooManyLines);

        var classes = await ClassesAsync(request.SchoolId, lines, cancellationToken);
        var itemIds = lines.Select(l => l.ItemId!.Value).Distinct().ToList();
        var references = await items.SaleReferencesAsync(itemIds, cancellationToken);
        var versions = await prices.VersionsOfItemsAsync(itemIds, request.OrderDate, cancellationToken);
        var made = new List<OrderLineInput>();
        var leftOut = new List<LeftOutLine>();
        foreach (var line in lines)
        {
            var className = ClassKey(line.ClassName) is { } key ? classes[key] : null;
            try
            {
                made.Add(LineOf(line, className, references, versions));
            }
            catch (ValidationFailureException refusal) when (!strict)
            {
                leftOut.Add(new LeftOutLine(line.ItemId, references.GetValueOrDefault(line.ItemId!.Value)?.Item.Code, className, refusal.Code));
            }
        }

        return new ComposedLines(made, leftOut);
    }

    /// <summary>R21: each class named, in the school's own spelling; a class the school does not have is refused naming it.</summary>
    private async Task<Dictionary<string, string>> ClassesAsync(Guid schoolId, IReadOnlyList<SchoolOrderLineRequest> lines, CancellationToken cancellationToken)
    {
        var resolved = new Dictionary<string, string>();
        foreach (var name in lines.Select(l => l.ClassName).Where(n => ClassKey(n) is not null).Distinct())
        {
            resolved[ClassKey(name)!] = await schools.ClassOfAsync(schoolId, name, cancellationToken)
                                        ?? throw new ValidationFailureException(SalesMessages.InvalidClass) { Subject = name };
        }

        return resolved;
    }

    private static string? ClassKey(string? name) => SchoolClass.KeyOf(name);

    private static OrderLineInput LineOf(
        SchoolOrderLineRequest line, string? className, IReadOnlyDictionary<Guid, ItemSaleReference> references, IReadOnlyDictionary<Guid, ItemPriceVersions> versions)
    {
        var reference = references.GetValueOrDefault(line.ItemId!.Value) ?? throw new ValidationFailureException(SalesMessages.ItemNotFound) { Subject = line.ItemId.ToString() };
        var subject = className is null ? reference.Item.Code : $"{reference.Item.Code} / {className}";
        if (!reference.Item.IsActive)
            throw new ValidationFailureException(SalesMessages.ItemInactive) { Subject = subject };

        var itemVersions = versions[reference.Item.Id];
        if (itemVersions.Versions.Count == 0)
            throw new ValidationFailureException(SalesMessages.NoPrice) { Subject = subject };
        var versionId = line.PriceVersionId ?? itemVersions.DefaultVersionId ?? itemVersions.Versions[0].Id;
        var version = itemVersions.Versions.FirstOrDefault(v => v.Id == versionId) ?? throw new ValidationFailureException(SalesMessages.InvalidPriceVersion) { Subject = subject };

        return new OrderLineInput(
            new SaleItem(reference.Item.Id, reference.Item.Code, reference.Item.Name, reference.HsnCode, reference.GstRatePercent),
            new SalePrice(version.Id, version.VersionName, version.MrpAmount, version.SaleAmount),
            line.Quantity!.Value,
            line.ItemDiscount?.ToDiscount() ?? Discount.None,
            className,
            line.LineId);
    }
}
