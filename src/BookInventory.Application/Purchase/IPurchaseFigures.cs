namespace BookInventory.Application.Purchase;

/// <summary>R8: the live vendor returns of a date range: how many and their total.</summary>
public sealed record VendorReturnFigure(int Count, decimal Total);

/// <summary>
/// R8: the capability the report module uses to read the vendor-return figures of the dashboard, never by joining the purchase
/// tables. It answers only for the session's company, returns plain values, leaves out a cancelled return and changes nothing.
/// </summary>
public interface IPurchaseFigures
{
    /// <summary>The live vendor returns dated from <paramref name="from"/> to <paramref name="to"/>, both included.</summary>
    Task<VendorReturnFigure> ReturnsAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
}
