namespace BookInventory.Domain.Customer;

/// <summary>R5: a customer is usable or switched off with its data kept. Never deleted.</summary>
public enum CustomerStatus
{
    Active,

    Inactive
}
