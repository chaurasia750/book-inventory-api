namespace BookInventory.Domain.Sales;

/// <summary>R40: what the counter knows about a student. The class is already the school's own spelling; the optional values may be empty.</summary>
public sealed record StudentDetails(string Name, string ClassName, string? Section = null, string? RollNumber = null, string? ParentMobile = null);
