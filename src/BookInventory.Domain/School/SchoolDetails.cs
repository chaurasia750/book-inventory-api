namespace BookInventory.Domain.School;

/// <summary>R2-R5, R7: the master data of a school as it arrives; <see cref="School"/> validates and normalises every value.</summary>
public sealed record SchoolDetails(
    string Name,
    SessionType SessionType,
    SaleType SaleType = SaleType.Both,
    bool IsGstDefault = false,
    decimal OpeningBalanceAmount = 0m,
    string? State = null,
    string? Address = null,
    string? ContactPerson = null,
    string? Mobile = null,
    string? Email = null,
    string? Gstin = null);
