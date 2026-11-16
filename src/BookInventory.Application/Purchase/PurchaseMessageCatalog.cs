using BookInventory.Domain.Purchase;

namespace BookInventory.Application.Purchase;

/// <summary>The human text of every <see cref="PurchaseMessages"/> code; the code travels, the text is looked up (R55 of iam).</summary>
public static class PurchaseMessageCatalog
{
    public static IReadOnlyDictionary<string, string> Messages { get; } =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [PurchaseMessages.VendorReturnNotFound] = "The vendor return could not be found.",
            [PurchaseMessages.VendorNotFound] = "The vendor could not be found.",
            [PurchaseMessages.VendorInactive] = "This vendor is inactive. Make it active to return books to it.",
            [PurchaseMessages.ItemNotFound] = "An item of the return could not be found.",
            [PurchaseMessages.ItemInactive] = "An item of the return is inactive.",
            [PurchaseMessages.NoPurchasePrice] = "An item has no purchase price on this date. Type its rate.",
            [PurchaseMessages.InvalidDate] = "The date cannot be in the future.",
            [PurchaseMessages.NoLines] = "Add at least one item to the return.",
            [PurchaseMessages.TooManyLines] = "A vendor return can have at most 500 lines.",
            [PurchaseMessages.InvalidRate] = "A rate must be above zero with at most two decimals.",
            [PurchaseMessages.InvalidQuantity] = "A quantity must be a whole number from 1.",
            [PurchaseMessages.InvalidSource] = "Choose Main or Damaged stock for each line.",
            [PurchaseMessages.InvalidText] = "The remark is too long.",
            [PurchaseMessages.InvalidReason] = "The reason must be 5 to 500 characters.",
            [PurchaseMessages.TotalNotPositive] = "The total of the return must be above zero.",
            [PurchaseMessages.AlreadyCancelled] = "The vendor return is already cancelled."
        };
}
