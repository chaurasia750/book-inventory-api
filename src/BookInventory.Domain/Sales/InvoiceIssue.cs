namespace BookInventory.Domain.Sales;

/// <summary>Spec sales R8-R10, R13: everything an invoice is made from: its number and date, the order, the company state and the delivery.</summary>
public sealed record InvoiceIssue
{
    public required string Number { get; init; }

    public required DateOnly InvoiceDate { get; init; }

    public required DateOnly Today { get; init; }

    public required SchoolOrder Order { get; init; }

    public required SaleSchool School { get; init; }

    public string? SchoolGstin { get; init; }

    public string? CompanyState { get; init; }

    public required OrderDelivery Delivery { get; init; }
}
