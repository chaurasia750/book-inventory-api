using BookInventory.Domain.Vendor;

namespace BookInventory.Application.Vendor;

/// <summary>The human text of every <see cref="VendorMessages"/> code; the code travels, the text is looked up (R55 of iam).</summary>
public static class VendorMessageCatalog
{
    public static IReadOnlyDictionary<string, string> Messages { get; } =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [VendorMessages.NotFound] = "The vendor could not be found.",
            [VendorMessages.NameTaken] = "A vendor with this name already exists.",
            [VendorMessages.InvalidName] = "The vendor name must be 2 to 150 characters.",
            [VendorMessages.InvalidField] = "A vendor detail is too long.",
            [VendorMessages.InvalidState] = "The state is not one of the states of India.",
            [VendorMessages.InvalidMobile] = "The mobile number must have 10 to 15 digits.",
            [VendorMessages.InvalidEmail] = "The email address is not valid.",
            [VendorMessages.InvalidGstin] = "The GSTIN must be 15 characters in the GST format.",
            [VendorMessages.InvalidBank] = "Give all five bank details (holder, bank, branch, a 9 to 18 digit account number and a valid IFSC) or none.",
            [VendorMessages.InvalidOpeningBalance] = "The opening balance must be zero or more, with at most two decimals.",
            [VendorMessages.AlreadyActive] = "The vendor is already active.",
            [VendorMessages.AlreadyInactive] = "The vendor is already inactive."
        };
}
