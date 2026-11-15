using BookInventory.Domain.Sales;

namespace BookInventory.Application.Sales;

/// <summary>The human text of every <see cref="SalesMessages"/> code; the code travels, the text is looked up (R55 of iam).</summary>
public static class SalesMessageCatalog
{
    public static IReadOnlyDictionary<string, string> Messages { get; } =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [SalesMessages.OrderNotFound] = "The order could not be found.",
            [SalesMessages.InvoiceNotFound] = "The invoice could not be found.",
            [SalesMessages.SchoolNotFound] = "The school could not be found.",
            [SalesMessages.SchoolInactive] = "The school is inactive; activate it before ordering.",
            [SalesMessages.ItemNotFound] = "Item not found.",
            [SalesMessages.ItemInactive] = "The item is inactive.",
            [SalesMessages.NoPrice] = "This item has no price yet; add a price version first.",
            [SalesMessages.InvalidPriceVersion] = "The price version does not belong to this item.",
            [SalesMessages.InvalidQuantity] = "A quantity is a whole number from 1 to 100000.",
            [SalesMessages.InvalidDiscount] = "A discount is a percentage from 0 to 100 or an amount of at least zero, with at most two decimals.",
            [SalesMessages.DiscountTooLarge] = "A rupee discount cannot be more than the amount it is taken from.",
            [SalesMessages.TooManyLines] = "An order has at most 500 lines.",
            [SalesMessages.InvalidDate] = "The date cannot be in the future (an invoice cannot be before its order).",
            [SalesMessages.InvalidSession] = "Choose the school's session of the order date, or the one before or after it.",
            [SalesMessages.InvalidText] = "The remark is at most 500 characters.",
            [SalesMessages.NotDraft] = "Only a draft order can be changed or deleted.",
            [SalesMessages.NoLines] = "Add at least one line before confirming the order.",
            [SalesMessages.NotConfirmed] = "Only a confirmed order can be invoiced or closed.",
            [SalesMessages.InvalidReason] = "The reason must be 5 to 500 characters.",
            [SalesMessages.NothingToInvoice] = "Choose at least one line with a quantity to invoice.",
            [SalesMessages.QuantityAboveRemaining] = "A quantity is more than what remains to deliver on that line.",
            [SalesMessages.TotalNotPositive] = "An invoice total must be above zero.",
            [SalesMessages.AlreadyCancelled] = "This invoice is already cancelled."
        };
}
