namespace BookInventory.Domain.Core;

/// <summary>
/// How a school pays (BRD §6, the Cash/Credit memo): <c>CASH</c> or <c>CREDIT</c>. A school has a default (school R20) that its orders
/// start from (sales R34); it decides the title of a GST-off invoice. Shared by the two modules, so there is one meaning.
/// </summary>
public enum PaymentType
{
    Cash,
    Credit
}
