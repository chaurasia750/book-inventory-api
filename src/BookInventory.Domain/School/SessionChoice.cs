namespace BookInventory.Domain.School;

/// <summary>R18: a school's session and the sessions before and after it (the user may pick those, sales R2).</summary>
public sealed record SessionChoice(string Session, string Previous, string Next);
