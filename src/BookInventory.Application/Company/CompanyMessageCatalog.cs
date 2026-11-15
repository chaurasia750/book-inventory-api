using BookInventory.Domain.Company;

namespace BookInventory.Application.Company;

/// <summary>The human text of every <see cref="CompanyMessages"/> code; the code travels, the text is looked up (R55 of iam).</summary>
public static class CompanyMessageCatalog
{
    public static IReadOnlyDictionary<string, string> Messages { get; } =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [CompanyMessages.NotFound] = "The company could not be found.",
            [CompanyMessages.NameTaken] = "A company with this name already exists.",
            [CompanyMessages.SlugUnavailable] = "This company name cannot be used: it gives an empty or a reserved web address.",
            [CompanyMessages.InvalidName] = "The company name must be 2 to 150 characters.",
            [CompanyMessages.InvalidField] = "A company detail is too long.",
            [CompanyMessages.OwnerIdInvalid] = "The company name needs at least 3 letters or digits to make a login id.",
            [CompanyMessages.ReasonRequired] = "A reason of 5 to 500 characters is required.",
            [CompanyMessages.AlreadyActive] = "The company is already active.",
            [CompanyMessages.AlreadyInactive] = "The company is already inactive.",
            [CompanyMessages.NotActive] = "The company is not active.",
            [CompanyMessages.InvalidBank] = "Give all five bank details (holder, bank, branch, account number, IFSC): an account number of 9 to 18 digits and a valid IFSC.",
            [CompanyMessages.LogoInvalid] = "The logo must be a PNG or JPEG image up to 2 MB.",
            [CompanyMessages.SignatureInvalid] = "The signature must be a PNG or JPEG image up to 2 MB.",
            [CompanyMessages.ImageNotFound] = "The company has no such image."
        };
}
