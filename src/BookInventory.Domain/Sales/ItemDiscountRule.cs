using BookInventory.BuildingBlocks.Domain.Exceptions;

namespace BookInventory.Domain.Sales;

/// <summary>
/// Spec sales R36: a line may carry its own discount only when the company uses item-wise discount (company R23). A line that
/// already carries a discount may keep it unchanged, so an order made before the setting was switched off can still be saved.
/// </summary>
public static class ItemDiscountRule
{
    /// <summary>Passes, or refuses with <c>400 sales.item_discount_disabled</c> naming the line (item code and class).</summary>
    public static void Require(bool companyUsesItemDiscount, OrderLineInput line, Discount? current)
    {
        if (companyUsesItemDiscount || line.ItemDiscount.Kind == DiscountKind.None || line.ItemDiscount == current)
            return;

        var subject = line.ClassName is null ? line.Item.Code : $"{line.Item.Code} / {line.ClassName}";
        throw new ValidationFailureException(SalesMessages.ItemDiscountDisabled) { Subject = subject };
    }
}
