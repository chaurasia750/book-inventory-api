namespace BookInventory.Domain.Company;

/// <summary>
/// Every message code of the <c>company</c> module (spec R55 of iam applies to all modules). The domain throws these
/// codes; the human text lives in <c>BookInventory.Application.Company.CompanyMessageCatalog</c>. Codes are stable API surface.
/// </summary>
public static class CompanyMessages
{
    public const string NotFound = "company.not_found";
    public const string NameTaken = "company.name_taken";
    public const string SlugUnavailable = "company.slug_unavailable";
    public const string InvalidName = "company.invalid_name";
    public const string InvalidField = "company.invalid_field";
    public const string OwnerIdInvalid = "company.owner_id_invalid";
    public const string ReasonRequired = "company.reason_required";
    public const string AlreadyActive = "company.already_active";
    public const string AlreadyInactive = "company.already_inactive";
    public const string NotActive = "company.not_active";
    public const string LogoInvalid = "company.logo_invalid";
    public const string SignatureInvalid = "company.signature_invalid";
    public const string ImageNotFound = "company.image_not_found";
}
