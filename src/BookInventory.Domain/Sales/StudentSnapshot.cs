namespace BookInventory.Domain.Sales;

/// <summary>Spec sales R13, R40: what a counter bill keeps of its student as it was when the bill was made (the section of that day, not the latest).</summary>
public sealed record StudentSnapshot(Guid Id, string Name, string ClassName, string? Section, string? RollNumber, string? ParentMobile);
