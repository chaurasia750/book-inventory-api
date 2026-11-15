namespace BookInventory.Domain.Company;

/// <summary>R21: the master data an Owner may change: everything of R3 except the name and the state; <see cref="Company"/> validates and normalises it.</summary>
public sealed record CompanyProfileDetails(
    string? Address = null,
    string? Mobile = null,
    string? Email = null,
    string? Gstin = null,
    string? Pan = null,
    string? Tagline = null);
