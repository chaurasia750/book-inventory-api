namespace BookInventory.Application.Planning;

/// <summary>Spec planning R2: what names one class book list: the school, the session and the class.</summary>
public sealed record BookListKey(Guid SchoolId, string Session, string ClassName);
