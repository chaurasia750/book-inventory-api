namespace BookInventory.Domain.Company;

/// <summary>R7: a company is usable (<see cref="Active"/>) or switched off with its data kept (<see cref="Inactive"/>). Never deleted.</summary>
public enum CompanyStatus
{
    Active,

    Inactive
}
