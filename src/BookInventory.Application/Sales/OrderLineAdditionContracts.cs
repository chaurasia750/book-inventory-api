using System.ComponentModel.DataAnnotations;
using BookInventory.Domain.Sales;

namespace BookInventory.Application.Sales;

/// <summary>R22: one class of a class set: the school's class and how many students buy the set.</summary>
public sealed record ClassSetRequest(
    [param: Required, StringLength(50)] string? ClassName,
    [param: Required, Range(1, 5000)] int? Students);

/// <summary>R22: the classes whose book lists are added to the order, each with its number of students.</summary>
public sealed record AddClassSetsRequest([param: Required, MinLength(1), MaxLength(30)] IReadOnlyList<ClassSetRequest>? Classes);

/// <summary>R23: many items added at once with one quantity, an optional class and an optional item discount.</summary>
public sealed record AddBulkLinesRequest(
    [param: Required, MinLength(1), MaxLength(200)] IReadOnlyList<Guid>? ItemIds,
    [param: Required, Range(1, SchoolOrder.MaxQuantity)] int? Quantity,
    [param: StringLength(50)] string? ClassName = null,
    DiscountRequest? ItemDiscount = null);

/// <summary>R22, R23, R25: a line that could not be added: the item, the class and the message code of the reason.</summary>
public sealed record LeftOutLine(Guid? ItemId, string? ItemCode, string? ClassName, string Reason);

/// <summary>R22, R23: how many lines were added (or raised) and which were left out.</summary>
public sealed record AddLinesResponse(int LinesAdded, IReadOnlyList<LeftOutLine> LeftOut);

/// <summary>The lines a request turned into, and those that were left out.</summary>
public sealed record ComposedLines(IReadOnlyList<OrderLineInput> Lines, IReadOnlyList<LeftOutLine> LeftOut);
