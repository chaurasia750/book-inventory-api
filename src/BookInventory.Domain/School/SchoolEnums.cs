namespace BookInventory.Domain.School;

/// <summary>R3: how the school's year runs (BRD FR-SCH-02). The company's financial year is always April-March.</summary>
public enum SessionType
{
    AprMar,

    JanDec
}

/// <summary>R4: what the company sells to this school (BRD FR-SCH-03).</summary>
public enum SaleType
{
    SchoolOrder,

    CounterSale,

    Both
}

/// <summary>R8: a school is usable or switched off with its data kept (BRD FR-SCH-08). Never deleted.</summary>
public enum SchoolStatus
{
    Active,

    Inactive
}
