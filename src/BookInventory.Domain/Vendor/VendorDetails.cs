using BookInventory.Domain.Core;
namespace BookInventory.Domain.Vendor;

/// <summary>R2-R6: the master data of a vendor as it arrives; <see cref="Vendor"/> validates and normalises every value.</summary>
public sealed record VendorDetails(
    string Name,
    string? State = null,
    string? Address = null,
    string? ContactPerson = null,
    string? Mobile = null,
    string? Email = null,
    string? Gstin = null,
    BankAccount? Bank = null,
    decimal OpeningBalanceAmount = 0m);
