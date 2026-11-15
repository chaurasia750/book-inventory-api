namespace BookInventory.Domain.Sales;

/// <summary>R38: what an Owner writes for a counter; <see cref="Counter"/> validates it. The session is already the school's own label.</summary>
public sealed record CounterDetails(Guid SchoolId, string Name, string Session, DateOnly StartDate, DateOnly? EndDate);
