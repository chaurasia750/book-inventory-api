namespace BookInventory.Domain.Customer;

/// <summary>R2-R7: the master data of a customer as it arrives; <see cref="Customer"/> validates and normalises every value.</summary>
public sealed record CustomerDetails(
    string Name,
    string? Mobile,
    string? Address = null,
    Guid? SchoolId = null,
    string? ClassName = null,
    decimal OpeningBalanceAmount = 0m,
    decimal? CreditLimitAmount = null);
