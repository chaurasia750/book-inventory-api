using BookInventory.Domain.Price;

namespace BookInventory.Application.Price;

/// <summary>The human text of every <see cref="PriceMessages"/> code; the code travels, the text is looked up (R55 of iam).</summary>
public static class PriceMessageCatalog
{
    public static IReadOnlyDictionary<string, string> Messages { get; } =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [PriceMessages.ItemNotFound] = "The item could not be found.",
            [PriceMessages.VersionNotFound] = "The price version could not be found.",
            [PriceMessages.NameTaken] = "This item already has a price version with this name.",
            [PriceMessages.InvalidName] = "The version name must be 2 to 30 characters.",
            [PriceMessages.InvalidAmount] = "A price must be zero or more, with at most two decimals.",
            [PriceMessages.InvalidChange] = "A price change needs a direction, a kind and a value of zero or more.",
            [PriceMessages.NothingSelected] = "No item matches the selection.",
            [PriceMessages.TooManyItems] = "At most 500 items can be asked at once."
        };
}
