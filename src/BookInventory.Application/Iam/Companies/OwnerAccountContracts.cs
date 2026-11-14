namespace BookInventory.Application.Iam.Companies;

/// <summary>
/// R61: what the <c>company</c> module hands over to create a company's Owner: the full name given by the Super Admin,
/// the user id it generated from the company's name, and the id of the company the account belongs to. Email and mobile
/// are optional for company accounts (R59).
/// </summary>
public sealed record OwnerRegistration(string FullName, string UserId, Guid CompanyId, string? Email = null, string? Mobile = null);

/// <summary>R57/R61: the new Owner. The temporary password exists only here and is never retrievable again.</summary>
public sealed record OwnerAccountCreated(Guid UserId, string UserName, string TemporaryPassword);
