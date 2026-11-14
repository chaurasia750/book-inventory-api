namespace BookInventory.Domain.Finance;

/// <summary>Spec finance R6: a party's opening balance moved from <see cref="OldAmount"/> to <see cref="NewAmount"/>.</summary>
public sealed record OpeningBalanceChange(decimal OldAmount, decimal NewAmount);
