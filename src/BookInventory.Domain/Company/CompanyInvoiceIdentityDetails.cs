namespace BookInventory.Domain.Company;

/// <summary>R19: the one bank account a company prints at the bottom of its documents; every value is optional.</summary>
public sealed record CompanyBankDetails(
    string? AccountHolder = null,
    string? BankName = null,
    string? AccountNumber = null,
    string? Ifsc = null,
    string? Branch = null,
    string? UpiId = null);

/// <summary>R20: the invoice settings a company prints: its terms and conditions, a footer line and whether the GSTIN shows.</summary>
public sealed record CompanyInvoiceSettings(string? Terms = null, string? Footer = null, bool PrintGstin = true);

/// <summary>R19/R20: the bank account and the invoice settings as they arrive; <see cref="Company"/> validates and normalises them.</summary>
public sealed record CompanyInvoiceIdentityDetails(CompanyBankDetails Bank, CompanyInvoiceSettings Invoice);
