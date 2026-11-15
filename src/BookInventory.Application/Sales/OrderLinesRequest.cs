namespace BookInventory.Application.Sales;

/// <summary>R3, R21: what the composer needs to turn request lines into order lines: the school, the date the prices are taken on and the lines.</summary>
public sealed record OrderLinesRequest(Guid SchoolId, DateOnly OrderDate, IReadOnlyList<SchoolOrderLineRequest> Lines);
