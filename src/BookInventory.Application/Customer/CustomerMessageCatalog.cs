using BookInventory.Domain.Customer;

namespace BookInventory.Application.Customer;

/// <summary>The human text of every <see cref="CustomerMessages"/> code; the code travels, the text is looked up (R55 of iam).</summary>
public static class CustomerMessageCatalog
{
    public static IReadOnlyDictionary<string, string> Messages { get; } =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [CustomerMessages.NotFound] = "The customer could not be found.",
            [CustomerMessages.InvalidName] = "The customer name must be 2 to 150 characters.",
            [CustomerMessages.InvalidField] = "A customer detail is too long.",
            [CustomerMessages.InvalidMobile] = "The mobile number must have 10 to 15 digits.",
            [CustomerMessages.MobileRequired] = "The mobile number is required.",
            [CustomerMessages.MobileTaken] = "A customer with this mobile number already exists.",
            [CustomerMessages.InvalidSchool] = "Choose an active school of this company.",
            [CustomerMessages.InvalidClass] = "Choose one of the classes of the chosen school.",
            [CustomerMessages.InvalidOpeningBalance] = "The opening balance must be zero or more, with at most two decimals.",
            [CustomerMessages.InvalidCreditLimit] = "The credit limit must be more than zero, with at most two decimals.",
            [CustomerMessages.AlreadyActive] = "The customer is already active.",
            [CustomerMessages.AlreadyInactive] = "The customer is already inactive."
        };
}
