namespace BookInventory.Domain.Company;

/// <summary>R3: the master data of a company as it arrives; <see cref="Company"/> validates and normalises every value.</summary>
public sealed record CompanyDetails(
    string Name,
    string? Address = null,
    string? Mobile = null,
    string? Email = null,
    string? Gstin = null,
    string? Pan = null,
    string? State = null,
    string? Tagline = null);
