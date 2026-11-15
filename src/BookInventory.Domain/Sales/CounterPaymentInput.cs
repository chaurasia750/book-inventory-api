namespace BookInventory.Domain.Sales;

/// <summary>Spec sales R45: one payment line of a counter bill as the request gave it.</summary>
public sealed record CounterPaymentInput(CounterPaymentMode Mode, decimal Amount, string? Reference = null);
